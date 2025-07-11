setInterval(() => {
    fetch('/Login/CheckSession?hashed=true', { cache: 'no-store' })
    .then(res => res.json())
    .then(data => {
        const role = data.role;
        const sessionHash = data.id;

        if (role === 'none') {
        window.location.href = '/Login/Index';
        return;
        }

        const queryString = window.location.search;

        const params = new URLSearchParams(queryString);
        const currentHash = params.get('h');

        const Hash = '@ViewData["Hash"]?.ToString()';

        const hashToCompare =  (currentHash && currentHash !== 'null') ? currentHash : Hash;

        if (!hashToCompare  || hashToCompare  !== sessionHash) {
        window.location.href = '/Login/Index';
        return;
        }
    })
    .catch(() => {
        window.location.href = '/Login/Index';
    });
}, 5000); // هر 5 ثانیه تا فشار به سرور نیاد
// این هر چند ثانیه چک میکند که هش سشن صفحه با هش سشن فعال یکی باشد
// اگر متفاوت باشد سشن این صفحه منقضی شده و به صفحه لاگین میرود
// اونجا اگر سشن فعال داریم به داشبوردش میرود اگر نداریم صفحه لاگین لود میشود
