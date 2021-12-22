// ==UserScript==
// @name         GM
// @namespace    
// @version      0.2
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
function GM_registerMenuCommand(){
	
}
function GM_setClipboard(val){
	copyTextToClipboard(val);
}

const copyTextToClipboard = (input, {target = document.body} = {}) => {
	const element = document.createElement('textarea');
	const previouslyFocusedElement = document.activeElement;

	element.value = input;

	// Prevent keyboard from showing on mobile
	element.setAttribute('readonly', '');

	element.style.contain = 'strict';
	element.style.position = 'absolute';
	element.style.left = '-9999px';
	element.style.fontSize = '12pt'; // Prevent zooming on iOS

	const selection = document.getSelection();
	let originalRange = false;
	if (selection.rangeCount > 0) {
		originalRange = selection.getRangeAt(0);
	}

	target.append(element);
	element.select();

	// Explicit selection workaround for iOS
	element.selectionStart = 0;
	element.selectionEnd = input.length;

	let isSuccess = false;
	try {
		isSuccess = document.execCommand('copy');
	} catch (_) {}

	element.remove();

	if (originalRange) {
		selection.removeAllRanges();
		selection.addRange(originalRange);
	}

	// Get the focus back on the previously focused element, if any
	if (previouslyFocusedElement) {
		previouslyFocusedElement.focus();
	}

	return isSuccess;
};

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

function GM_listValues() {
    var names = [];
    for (var i = 0, len = localStorage.length; i < len; i++) 
    {
        names.push(localStorage.key(i));
    }
    return names;
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
