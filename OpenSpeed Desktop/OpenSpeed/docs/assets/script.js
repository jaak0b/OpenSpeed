(() => {
  const STORAGE_KEY = 'openspeed-lang';
  const DEFAULT_LANG = 'en';

  function getLang() {
    return localStorage.getItem(STORAGE_KEY) || DEFAULT_LANG;
  }

  function setLang(lang) {
    localStorage.setItem(STORAGE_KEY, lang);
    document.documentElement.className = 'lang-' + lang;

    const btn = document.getElementById('lang-toggle');
    if (btn) btn.textContent = lang === 'en' ? 'DE' : 'EN';

    document.documentElement.lang = lang === 'de' ? 'de' : 'en';
  }

  function toggle() {
    setLang(getLang() === 'en' ? 'de' : 'en');
  }

  document.addEventListener('DOMContentLoaded', () => {
    setLang(getLang());

    const btn = document.getElementById('lang-toggle');
    if (btn) btn.addEventListener('click', toggle);

    document.querySelectorAll('.nav-links a').forEach(a => {
      a.addEventListener('click', e => {
        e.preventDefault();
        const target = document.querySelector(a.getAttribute('href'));
        if (target) target.scrollIntoView({ behavior: 'smooth' });
      });
    });
  });
})();
