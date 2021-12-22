// ==UserScript==
// @name         GM
// @namespace    
// @version      0.31
// @description 需要更新程序至2.0.0.8 基础脚本框架(不建议取消勾选，会导致某些脚本无法运行)
// @author       软妹币玩家、wsz987
// @match        *
// @enable      true
// @grant        GM_xmlhttpRequest
// @grant        GM_addStyle
// @grant        GM_getValue
// @grant        GM_setValue
// @grant        GM_log
// ==/UserScript==

var unsafeWindow = this;

function GM_registerMenuCommand() {

}
function GM_xmlhttpRequest(option) {
    if (String(option) !== '[object Object]') return undefined
    option.method = option.method ? option.method.toUpperCase() : 'GET'
    option.data = option.data || {}
    var formData = []
    for (var key in option.data) {
        formData.push(''.concat(key, '=', option.data[key]))
    }
    option.data = formData.join('&')
    if (option.method === 'GET'&&option.data!=null&&option.data.length>0) {
        option.url += location.search.length === 0 ? ''.concat('?', option.data) : ''.concat('&', option.data)
    }
    var url='https://local.steampp.net/?request='+encodeURIComponent(option.url);
    option.url=url;
    var xhr = new XMLHttpRequest();
    xhr.timeout = option.timeout;
    xhr.responseType = option.responseType || 'text'
    xhr.onerror = option.onerror;
    xhr.ontimeout = option.ontimeout;
    xhr.open(option.method, option.url, true);
	xhr.setRequestHeader('requestType', 'xhr'); 
    if (option.method === 'POST') {
        xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded')
    }
    if (option.headers) {
        Object.keys(option.headers).forEach(function (key) {
			try{
				if(key.toLowerCase()=='cookie')
            xhr.setRequestHeader('cookie-steamTool', option.headers[key]);
		else
            xhr.setRequestHeader(key, option.headers[key]);
			}catch{}
        });
    } 
    if(option.responseType=='json'){
		 xhr.setRequestHeader('Content-Type', 'application/json; charset='+document.charset)
	}
	xhr.onload = (e) => {
		console.log(e)
        if (option.onload && typeof option.onload === 'function') {
            option.onload(e.target)
        }
    };
    xhr.send(option.method === 'POST' ? option.data : null)
}

function GM_addStyle(css) {
    try {
        let style = document.createElement('style');
        style.textContent = css;
        (document.head || document.body || document.documentElement || document).appendChild(style);
    } catch (e) {
        console.log("Error: env: adding style " + e);
    }
}

function GM_getValue(name, defaultValue) {
    let value = window.localStorage.getItem(name)
    if (!value) {
        return defaultValue;
    }
    let type = value[0];
    value = value.substring(1);
    switch (type) {
        case 'b':
            return value == 'true';
        case 'n':
            return Number(value);
        case 'o':
            try {
                return JSON.parse(value);
            } catch (e) {
                console.log("Error: env: GM_getValue " + e);
                return defaultValue;
            }
            default:
                return value;
    }
}

function GM_setValue(name, value) {
    let type = (typeof value)[0];
    switch (type) {
        case 'o':
            try {
                value = type + JSON.stringify(value);
            } catch (e) {
                console.log("Error: env: GM_setValue typeof ?Object" + e);
                return;
            }
            break;
        default:
            value = type + value;
    }
    try {
        if (typeof name !== 'string') JSON.stringify(name)
        localStorage.setItem(name, value);
    } catch (e) {
        console.log("Error: env: GM_setValue saveing" + e);
    }
}

function GM_log(message) {
    if (window.console) {
        window.console.log(message);
    } else {
        console.log(message);
    }
}

function GM_listValues() {
    var names = [];
    for (var i = 0, len = localStorage.length; i < len; i++) {
        names.push(localStorage.key(i));
    }
    return names;
}

function onlySteam() {
    var el = document.getElementById('global_actions');
    if (el == null) {
        var box = document.getElementById("footer");
        if (box!=null) {
            var html = '<div id="global_actions"><div id="global_action_menu"></div></div>';
            var item = document.createElement('div');
            item.innerHTML = html;
            box.append(item);
        }
    }
}
onlySteam();