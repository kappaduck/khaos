// khaos.ts - starts a Khaos game in the page.
//
// To start it yourself, mark the script tag <script type="module" src="_khaos/khaos.js" data-autostart="false"></script>
// and call await Khaos.start({ canvas: '#my-canvas', clickToPlay: false, args: ['--lives', '5'] });
//
// The page holds the markup and _khaos/khaos.css. index.html is the default page,
// and the template copies it into the game's wwwroot/. This file only finds the
// elements, by their attribute, and says which screen is up:
//
//   [data-khaos-host]            The element around the canvas (default: the
//                                canvas's parent). khaos.js sets its
//                                data-khaos-state: loading, ready (waiting for
//                                Play), playing or failed. khaos.css shows the
//                                [data-khaos-screen] of that state.
//   <canvas id="canvas">         Or options.canvas. Created in the host if the
//                                page has none.
//   [data-khaos-play]            Starts the game. Without one, a click on the
//                                host or Enter starts it.
//   [data-khaos-error-message]   Receives what went wrong, and
//   [data-khaos-error-details]   the stack or the game's exception.
//   [data-khaos-fullscreen]      Puts the host in fullscreen and back. Hidden
//                                when the browser has no fullscreen.
//
// Every one is optional: a page with only a canvas runs the game, without
// screens, and its errors stay in the console.
//
// The game follows the canvas's CSS size, so a game with GameSettings.Resizable and
// a scaling mode fills whatever box the page gives it, letterboxed.
import { dotnet } from '../_framework/dotnet';
import type { LoadBootResourceCallback, RuntimeAPI, WebAssemblyBootResourceType } from '../_framework/dotnet'


/**
 * How {@link start} runs the game.
 */
export interface StartOptions {
  /**
   * The canvas the game draws on: the element itself, or a CSS selector for it.
   *
   * @remarks
   * Without this option, the page's `<canvas id="canvas">` is used, or one is created in
   * the host when the page has none. The canvas's parent is the host of the screens unless
   * the page marks another element with `data-khaos-host`.
   *
   * Khaos finds the canvas by its id: a canvas without one gets `canvas` (or
   * `khaos-canvas` when the page already has a `#canvas`).
   *
   * A selector that matches no `<canvas>`, or a `#canvas` that is another kind of
   * element, is an error: {@link start} rejects with the reason, also written to the
   * console.
   *
   * @defaultValue `#canvas`, created if missing
   */
  canvas?: string | HTMLCanvasElement;

  /**
   * Waits for the player to press Play (or Enter, or Space) before `Main` runs.
   *
   * @remarks
   * Browsers keep a page's audio muted until the player interacts with it: without this
   * wait, the game's first sounds are lost. Turn it off for a page that has already had
   * that interaction, such as one that starts the game from its own button.
   *
   * @defaultValue `true`
   */
  clickToPlay?: boolean;

  /**
   * Arguments for the game's `Main`, after the ones read from the page's query string
   * (`?arg=--lives&arg=5`).
   *
   * @defaultValue none
   */
  args?: string[];
}

/**
 * The `window.Khaos` global, for pages that start the game themselves.
 *
 * @example
 * ```html
 * <script type="module" src="_khaos/khaos.js" data-autostart="false"></script>
 * <script type="module">
 *   document.querySelector('#start').addEventListener('click', () => {
 *     Khaos.start({ clickToPlay: false });
 *   });
 * </script>
 * ```
 */
export interface KhaosGlobal {
  /** {@inheritDoc start} */
  start(options?: StartOptions): Promise<RuntimeAPI>;
}

declare global {
  interface Window {
    /** Starts a Khaos game in the page: see {@link KhaosGlobal}. Set as soon as khaos.js loads. */
    Khaos: KhaosGlobal;
  }
}

// Emscripten's Module has a canvas property SDL reads; .NET's typings leave it out.
type SdlModule = RuntimeAPI['Module'] & { canvas: HTMLCanvasElement };

// GameRunner prints this before the exception when the game stops on an error.
const stoppedMarker = 'Khaos: the game stopped';

let started: Promise<RuntimeAPI> | undefined;

/**
 * Starts the .NET runtime, waits for Play, then runs the game's `Main`.
 *
 * @remarks
 * khaos.js calls it on its own unless its script tag has `data-autostart="false"`.
 * Safe to call more than once: the first call wins, and later ones return its promise
 * whatever options they pass. Failures are shown on the page (the `failed` screen) as
 * well as rejected.
 *
 * @param options - How to run the game; see {@link StartOptions}.
 * @returns The .NET runtime API, once `Main` has returned. In the browser `Main` returns
 * as soon as the first frame is scheduled: the game keeps running after that.
 * @throws The runtime's error when it cannot start: a file the page does not serve, or a
 * browser too old for .NET's WebAssembly.
 *
 * @example
 * ```ts
 * const runtime = await Khaos.start({ canvas: '#game', args: ['--level', '2'] });
 * ```
 */
export function start(options: StartOptions = {}): Promise<RuntimeAPI> {
  started ??= run(options);
  return started;
}

async function run(options: StartOptions): Promise<RuntimeAPI> {
  let ui: Ui;

  try {
    ui = connectUi(options);
  } catch (error) {
    console.error(error);
    throw error;
  }

  reportGameErrors(ui);

  try {
    const queryArgs = new URLSearchParams(window.location.search).getAll('arg');
    let builder = dotnet.withApplicationArguments(...queryArgs, ...(options.args ?? []));

    if (ui.canvas.id !== 'canvas') {
      builder = builder.withEnvironmentVariable('SDL_EMSCRIPTEN_CANVAS_SELECTOR', `#${ui.canvas.id}`);
    }

    const encoding = await detectEncoding();

    if (encoding) {
      builder = builder.withResourceLoader(precompressedLoader(encoding));
    }

    const runtime = await builder.create();

    // SDL changes the mouse cursor through Module.canvas.
    (runtime.Module as SdlModule).canvas = ui.canvas;

    if (options.clickToPlay ?? true) {
      await ui.waitForPlay();
    }

    ui.playing();

    const exitCode = await runtime.runMain();

    if (exitCode !== 0) {
      ui.fail(`The game did not start (exit code ${exitCode}).`, 'The browser console (F12) has the details.');
    }

    return runtime;
  } catch (error) {
    console.error(error);
    ui.fail('The game could not start.', describe(error));

    throw error;
  }
}

/** The value of data-khaos-state on the host; khaos.css shows the screen of the same name. */
type State = 'loading' | 'ready' | 'playing' | 'failed';

/** The screens of the page, as run() drives them. */
interface Ui {
  /** The canvas the game draws on. */
  readonly canvas: HTMLCanvasElement;
  /** Shows the ready screen and resolves on the player's first interaction. */
  waitForPlay(): Promise<void>;
  /** Hides the screens and gives the canvas the keyboard, unless the game has already failed. */
  playing(): void;
  /** Shows the failed screen. Only the first report counts: it is the most specific one. */
  fail(message: string, details?: string): void;
}

function connectUi(options: StartOptions): Ui {
  const found = resolveCanvas(options.canvas);
  const host = resolveHost(found);

  const canvas = found ?? createCanvas(host);

  canvas.id ||= document.getElementById('canvas') ? 'khaos-canvas' : 'canvas';
  canvas.tabIndex = -1;
  canvas.addEventListener('contextmenu', (e) => e.preventDefault());

  const setState = (state: State): void => {
    host.dataset.khaosState = state;
  };

  const fullscreen = host.querySelector<HTMLButtonElement>('[data-khaos-fullscreen]');

  if (fullscreen) {
    fullscreen.hidden = !document.fullscreenEnabled;

    fullscreen.addEventListener('click', () => {
      if (document.fullscreenElement) {
        void document.exitFullscreen();
      } else {
        void host.requestFullscreen();
      }

      canvas.focus();
    });
  }

  let failed = false;

  setState('loading');

  return {
    canvas,

    waitForPlay() {
      setState('ready');

      const play = host.querySelector<HTMLElement>('[data-khaos-play]');

      return new Promise<void>((resolve) => {
        const listening = new AbortController();
        const signal = listening.signal;

        const go = (): void => {
          listening.abort();
          resolve();
        };

        // Enter or Space count as the interaction that unlocks audio too.
        window.addEventListener('keydown', (e: KeyboardEvent) => {
          if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            go();
          }
        }, { signal });

        if (play) {
          play.addEventListener('click', go, { signal });
          play.focus();
        } else {
          // No Play button on the page: the host is one.
          host.addEventListener('pointerdown', go, { signal });
        }
      });
    },

    playing() {
      if (!failed) {
        setState('playing');
      }

      canvas.focus();
    },

    fail(message, details) {
      if (failed) {
        return;
      }

      failed = true;
      if (document.fullscreenElement) {
        document.exitFullscreen().catch(() => { });
      }

      setText(host, '[data-khaos-error-message]', message);
      setText(host, '[data-khaos-error-details]', details ?? '');
      setState('failed');
    },
  };
}

/** The canvas the options or the page name, or null when there is none. */
function resolveCanvas(canvas: StartOptions['canvas']): HTMLCanvasElement | null {
  if (canvas instanceof HTMLCanvasElement) {
    return canvas;
  }

  const selector = canvas ?? '#canvas';
  const found = document.querySelector(selector);

  if (found instanceof HTMLCanvasElement) {
    return found;
  }

  if (found) {
    throw new Error(`Khaos: '${selector}' is a <${found.localName}>, not a <canvas>.`);
  }

  if (canvas !== undefined) {
    throw new Error(`Khaos: no element matches '${selector}' (StartOptions.canvas).`);
  }

  return null;
}

function resolveHost(canvas: HTMLCanvasElement | null): HTMLElement {
  const marked = document.querySelector<HTMLElement>('[data-khaos-host]');

  if (marked) {
    return marked;
  }

  if (canvas?.parentElement) {
    return canvas.parentElement;
  }

  document.documentElement.classList.add('khaos-page');

  const host = document.createElement('div');
  host.className = 'khaos-host';

  document.body.append(host);
  return host;
}

function createCanvas(host: HTMLElement): HTMLCanvasElement {
  const canvas = document.createElement('canvas');

  canvas.id = 'canvas';
  host.prepend(canvas);

  return canvas;
}

function setText(host: HTMLElement, selector: string, text: string): void {
  const element = host.querySelector(selector);

  if (element) {
    element.textContent = text;
  }
}

function describe(error: unknown): string {
  return error instanceof Error ? (error.stack ?? error.message) : String(error);
}

/**
 * Puts the errors that have no caller on the page.
 *
 * @remarks
 * A game that throws inside the loop has no caller to throw to: GameRunner prints the
 * exception, marked, and this shows it. The WebAssembly side failing outright (an abort)
 * arrives as an uncaught RuntimeError.
 */
function reportGameErrors(ui: Ui): void {
  const original = console.error.bind(console);

  console.error = (...args: unknown[]): void => {
    original(...args);

    const text = args.map(String).join(' ');
    if (text.includes(stoppedMarker)) {
      ui.fail('The game stopped because of an error.', text);
    }
  };

  window.addEventListener('error', (e: ErrorEvent) => {
    if (e.error instanceof WebAssembly.RuntimeError) {
      ui.fail('The game crashed.', describe(e.error));
    }
  });
}

type Encoding = '.br' | '.gz';

/**
* Finds the precompressed copies the host decodes for the browser, if any.
*
* @remarks
* probe.txt holds "khaos". A host that serves probe.txt.br with
* Content-Encoding: br (itch.io, nginx with brotli_static, IIS) hands back the
* decoded text; one that serves the raw bytes hands back gibberish, and a zip
* published with KhaosWebCompression=None has no .br at all. The first
* extension that reads back as "khaos" is the one the host decodes.
*
* @returns The extension to ask for, or null to stay on the plain files.
*/
async function detectEncoding(): Promise<Encoding | null> {
  for (const extension of ['.br', '.gz'] as const) {
    try {
      const response = await fetch(new URL(`./probe.txt${extension}`, import.meta.url), { cache: 'no-store' });

      if (response.ok && (await response.text()).trim() === 'khaos') {
        return extension;
      }
    } catch {
      // Not served: try the next one.
    }
  }

  return null;
}

// Content types the runtime needs on a resource it streams:
// WebAssembly.instantiateStreaming refuses anything but application/wasm,
// and a host may label dotnet.native.wasm.br as something else.
const streamedContentTypes: Partial<Record<WebAssemblyBootResourceType, string>> = {
  dotnetwasm: 'application/wasm',
};

/**
 * A resource loader for the runtime that asks for the precompressed copy of each file.
 *
 * @remarks
 * The runtime's JavaScript modules are loaded with import(), which checks the MIME type
 * of the URL itself: those stay on the plain files (null lets the runtime load them).
 */
function precompressedLoader(extension: Encoding): LoadBootResourceCallback {
  return (type, _name, defaultUri) => (type === 'dotnetjs' ? null : fetchPrecompressed(type, defaultUri, extension));
}

/** Fetches the precompressed copy of a boot resource, or the plain file when the host has none. */
async function fetchPrecompressed(type: WebAssemblyBootResourceType, defaultUri: string, extension: Encoding): Promise<Response> {
  // Before the query string: dotnet.native.wasm?v=… becomes dotnet.native.wasm.br?v=…
  const url = new URL(defaultUri, document.baseURI);
  url.pathname += extension;

  const response = await fetch(url);

  if (!response.ok) {
    // No precompressed copy of this one: take the plain file.
    return fetch(defaultUri);
  }

  // The browser has already decoded the body: drop the headers that
  // describe the encoded bytes, and set the type the runtime expects.
  const headers = new Headers(response.headers);
  headers.delete('content-encoding');
  headers.delete('content-length');

  const contentType = streamedContentTypes[type];

  if (contentType) {
    headers.set('content-type', contentType);
  }

  return new Response(response.body, { status: response.status, headers });
}

window.Khaos = { start };

const self = document.querySelector<HTMLScriptElement>('script[type="module"][src*="khaos.js"]');

if (self?.dataset.autostart !== 'false') {
  // Failures are already on the page; the rejection only matters to a caller.
  start().catch(() => { });
}
