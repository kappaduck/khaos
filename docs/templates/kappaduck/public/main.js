export default {
  iconLinks: [
    {
      icon: 'github',
      href: 'https://github.com/kappaduck/khaos',
      title: 'GitHub'
    },
    {
      icon: 'box-seam',
      href: 'https://www.nuget.org/packages/KappaDuck.Khaos',
      title: 'NuGet'
    }
  ],
  start: () => {
    for (const year of document.querySelectorAll('.copyright-year'))
      year.textContent = new Date().getFullYear()

    openIconLinksInNewTab()
    joinTypeSuffixes()
  }
}

function openIconLinksInNewTab() {
  const navbar = document.getElementById('navbar')
  if (!navbar)
    return

  const mark = () => {
    for (const link of navbar.querySelectorAll('form.icons a[href^="http"]')) {
      link.target = '_blank'
      link.rel = 'noopener'
    }
  }

  mark()
  new MutationObserver(mark).observe(navbar, { childList: true, subtree: true })
}

function joinTypeSuffixes() {
  for (const link of document.querySelectorAll('article a.xref')) {
    const next = link.nextSibling
    if (next?.nodeType !== Node.TEXT_NODE)
      continue

    const suffix = /^(?:\[,*\]|\?)+/.exec(next.data)
    if (!suffix)
      continue

    link.append(suffix[0])
    next.data = next.data.slice(suffix[0].length)
  }
}
