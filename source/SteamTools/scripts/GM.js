// ==UserScript==
// @name         GM
// @namespace    
// @version      0.1
// @description  基础脚本框架(不建议取消勾选，会导致某些脚本无法运行)
// @author       软妹币玩家
// @match        *
// @enable      true
// @grant        GM_xmlhttpRequest
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
        if (option.onload && typeof option.onload === 'function')
        {
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