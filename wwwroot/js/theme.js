(function(){
    window.theme = {
        apply: function(name) {
            if(!name) return;
            document.documentElement.setAttribute('data-theme', name);
            try { localStorage.setItem('theme', name); } catch(e) {}
        },
        get: function(){
            try { return localStorage.getItem('theme') || 'tenue'; } catch(e){ return 'tenue'; }
        },
        init: function(){ this.apply(this.get()); }
    };
})();
