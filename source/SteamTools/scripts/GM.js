// ==UserScript==
// @name         GM
// @namespace    
// @version      0.1
// @description  基础脚本框架(不建议取消勾选，会导致某些脚本无法运行)
// @author       软妹币玩家、wsz987
// @match        *
// @enable      true
// @grant        GM_xmlhttpRequest
// @grant        GM_addStyle
// @grant        GM_getValue
// @grant        GM_setValue
// ==/UserScript==

var unsafeWindow = this;

function GM_xmlhttpRequest(option) {
    if (String(option) !== '[object Object]') return undefined
    option.method = option.method ? option.method.toUpperCase() : 'GET'
    option.data = option.data || {}
    var formData = []
    for (var key in option.data) {
        formData.push(''.concat(key, '=', option.data[key]))
    }
    option.data = formData.join('&')

    if (option.method === 'GET') {
        option.url += location.search.length === 0 ? ''.concat('?', option.data) : ''.concat('&', option.data)
    }

    var xhr = new XMLHttpRequest();
    xhr.timeout = option.timeout;
    xhr.responseType = option.responseType || 'json'
    xhr.onload = (e) => {
        if (option.onload && typeof option.onload === 'function') {
            option.onload(e.target)
        }
    };
    xhr.onerror = option.onerror;
    xhr.ontimeout = option.ontimeout;
    xhr.open(option.method, option.url, true)
    if (option.method === 'POST') {
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded')
    }
    if (option.headers) {
        Object.keys(option.headers).forEach(function (key) {
            xhr.setRequestHeader(key, option.headers[key]);
        });
    }
    xhr.send(option.method === 'POST' ? option.data : null)
}

function GM_addStyle(css) {
    let head, style;
    head = document.getElementsByTagName('head')[0];
    if (!head) return
    style = document.createElement('style');
    style.type = 'text/css';
    style.innerHTML = css;
    head.appendChild(style);
}

function GM_getValue(name) {
    let val = window.localStorage.getItem(name)
    if (val === null) return undefined
    if (val==='[]') return []
    return isJSON(val) ? JSON.parse(val) : eval(val)
}

function GM_setValue(name, val) {
    if(typeof str !== 'string') JSON.stringify(name)
    localStorage.setItem(name, JSON.stringify(val));
}

function isJSON(str) {
    if (typeof str == 'string') {
        try {
            let obj = JSON.parse(str);
            if (typeof obj == 'object' && obj) {
                return true;
            } else {
                return false;
            }
        } catch (e) {
            return false;
        }
    }
}