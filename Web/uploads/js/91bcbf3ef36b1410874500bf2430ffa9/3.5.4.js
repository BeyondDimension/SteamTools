// ==UserScript==
// @name                划词翻译-冻猫（更换有道翻译）
// @name:ja             翻訳
// @name:zh-CN          划词翻译
// @namespace           http://www.icycat.com
// @description         选中文字自动翻译
// @description:ja      選択した文字の自動翻訳
// @description:zh-CN   选中文字自动翻译
// @author              alex23
// @include             *
// @version             3.5.4
// @grant 		GM_xmlhttpRequest
// @grant 		GM_addStyle
// @grant               GM_getValue
// @grant               GM_setValue
// @require             https://cdn.bootcss.com/jquery/1.12.4/jquery.min.js
// @run-at              document-start
// ==/UserScript==
    'use strict';

    var gv = {};

    if (!GM_getValue('toLanguage')) {
        if (/zh/i.test(navigator.language)) {
            GM_setValue('toLanguage', 'zh-CN');
        } else if (/ja/i.test(navigator.language)) {
            GM_setValue('toLanguage', 'ja');
        } else if (/en/i.test(navigator.language)) {
            GM_setValue('toLanguage', 'en');
        }
    }

    if (!GM_getValue('apiHost')) {
        GM_setValue('apiHost', 'translate.google.cn');
    }

    function init() {

        document.addEventListener('mousedown', mouseStart, true);
        document.addEventListener('mouseup', mouseEnd, true);

        function mouseStart(e) {
            if ($('#catTranslateBox').length == 0) {
                createBox();
                $('#catTranslateBox li').on('click', setLanguage);
            }
            if ($('#catTranslateBox').css('display') == 'block' && !checkClick(e)) {
                clearTranslate()
            }
            moveCheck();
            if (e.target.className == 'catPlaySound') {
                $('.catPlaySound').addClass('catPlaySoundClick');
                playSound();
            } else if (e.target.className == 'catSet') {
                $('.catdropdown').css('display', 'block');
            }
        }

        function moveCheck(e) {
            clearTimeout(gv.timer);
            gv.holdTime = false;
            gv.timer = setTimeout(function() {
                gv.holdTime = true;
            }, 1000);
        }

        function mouseEnd(e) {
            if (gv.holdTime == true && window.getSelection().toString()) {
                e.preventDefault()
                e.stopPropagation();
                gv.holdTime = false;
                showBox(e.clientX, e.clientY);
                gv.selectText = window.getSelection().toString();
                gv.encodeText = encodeURIComponent(gv.selectText.replace(/([a-z])([A-Z])/g, '$1 $2').replace(/([A-Z]+)([A-Z][a-z])/g, '$1 $2'));
                ttranslate();
            }
            if ($('.catPlaySoundClick').length > 0) {
                $('.catPlaySound').removeClass('catPlaySoundClick');
            }
            clearTimeout(gv.timer);
        }
    }

    function setLanguage(e) {
        GM_setValue('toLanguage', e.target.getAttribute('name'))
        $('.catdropdown').css('display', '');
        ttranslate('auto');
        $('.catText').text(e.target.innerText + 'OK!');
        setTimeout(function() {
            $('.catText').empty();
        }, 1000);
    }

    function createBox() {
        GM_addStyle([
            '#catTranslateBox * {margin:0;padding:0;box-sizing:border-box;}',
            '#catTranslateBox {min-height:24px;min-width:100px;max-width:360px;font:normal 12px/24px Helvetica, Tahoma, Arial, sans-serif;text-align: left;position: absolute;z-index: 2147483647;top: 22px;left: -35px;background: #fff;border: 1px solid #dcdcdc;-webkit-transition: opacity .218s;transition: opacity .218s;box-shadow: 0 1px 4px rgba(0,0,0,.2);padding: 5px 0;display: none;font-size: 12px;line-height: 20px;border-radius:3px;}',
            '#catTranslateBox .catContentBox {margin:0 8px;color:#333;}',
            '#catTranslateBox .catContentBox .catTextBox{line-height:16px;border-bottom: 1px solid #ccc;padding: 2px 18px 9px 0;height: 25px;}',
            '#catTranslateBox .catContentBox .catTextBox div{vertical-align: middle;margin-right: 4px;color:#333;font-weight: normal;font-size:12px;}',
            '#catTranslateBox .catContentBox .catTextBox .catText{display: inline-block;font-size:14px;font-weight: bold;color:#333;}',
            '#catTranslateBox .catContentBox .catTextBox .catPlaySound {margin-left: 1px;cursor:pointer;display: inline-block;vertical-align: middle;width: 14px;height: 11px;overflow: hidden;background: url("data:image/gif;base64,R0lGODlhDgAZAIAAABy3/f///yH5BAAAAAAALAAAAAAOABkAAAI1jA+nC7ncXmg0RlTvndnt7jlcxkmjqWzotLbui4qxqBpUmoDl2Nk5GOKRSsCfDyer7ZYMSQEAOw==") no-repeat;text-decoration: none;}',
            '#catTranslateBox .catContentBox .catTextBox .catPlaySound.catPlaySoundClick {background-position:0 -14px;}',
            '#catTranslateBox .catContentBox .catExplain{padding: 2px 0 0 0;font-weight: normal;font-size:12px;}',
            '#catTranslateBox .catTipArrow {width: 0;height: 0;font-size: 0;line-height: 0;display: block;position: absolute;top: -16px;left: 10px;}',
            '#catTranslateBox .catTipArrow em, #catTranslateBox .catTipArrow ins {width: 0;height: 0;font-size: 0;line-height: 0;display: block;position: absolute;border: 8px solid transparent;border-style: dashed dashed solid;}',
            '#catTranslateBox .catTipArrow em {border-bottom-color: #d8d8d8;font-style: normal;color: #c00;}',
            '#catTranslateBox .catTipArrow ins {border-bottom-color: #fff;top: 2px;text-decoration: underline;background:none !important}',
            '#catTranslateBox .catSet {position:absolute;top:9px;right:10px;cursor: pointer;width: 14px;height: 14px;background: url("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAAOCAMAAAAolt3jAAAAdVBMVEUAAAAwi/+Zxv9urv9oq/9pq/9Elv81jf89kv8wiv8+kv8wiv8xi/8wi/9/t/9+t/9co/9Zof9Hl/9Gl/9ClP9Bk/85j//k8f/Z6v+Fu//x+P/e7f/b6//G3/+/2/+w0/+q0P+52P+42P+Lvv+IvP9wr/9vr/864/KKAAAAF3RSTlMAR/7s7OK7l5VuTyMTC/z7y8ihnYSAQ/Vmp/0AAAB9SURBVAjXVY9HDsQwDAPpVKf32E7v/3/iGtJhk7kNIIgkLH0QA3EgQHTX7AnhzWdLKo9hMWYZdkmaFFpZdJ6QRo7afH9TTmSlqcy4hmkarqMpazxq0m4GZK6e1I37rQ/q8n9cNZ9XHJRzUMFBcucaB9doTy55dSAET+gB/ABPjgqB+Q/YPgAAAABJRU5ErkJggg==") no-repeat;text-decoration: none;}',
            '#catTranslateBox .catSet .catdropdown {margin:0;padding:0;display:none;top:13px;right:-60px;position: absolute;background-color: #ffffff;width: 68px;overflow: auto;z-index: 1;border: 1px solid rgba(0,0,0,.2);box-shadow: 0 2px 4px rgba(0,0,0,.2);}',
            '#catTranslateBox .catSet .catdropdown li {list-style-type:none; color: black;padding: 6px 8px;margin:0px;text-decoration: none;display: block;text-align:center;}',
            '#catTranslateBox .catSet .catdropdown li:hover { background-color: #f1f1f1;}'
        ].join('\n'));
        $(
            '<div id="catTranslateBox">' +
            '<div class="catContentBox">' +
            '<div class="catTextBox">' +
            '<div class="catText"></div>' +
            '<div class="catPlaySound"></div>' +
            '</div>' +
            '<div class="catExplainBox">' +
            '<div class="catExplain"></div>' +
            '<div class="catPlaySound"></div>' +
            '</div>' +
            '</div>' +
            '<div class="catTipArrow"><em></em><ins></ins></div>' +
            '<div class="catSet">' +
            '<ul class="catdropdown">' +
            '<li name="AUTO">自动检测语言</li>'+
            '<li name="zh-CHS2en">中文&nbsp; » &nbsp;英语</li>'+
            '<li name="en2zh-CHS">英语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2ja">中文&nbsp; » &nbsp;日语</li>'+
            '<li name="ja2zh-CHS">日语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2ko">中文&nbsp; » &nbsp;韩语</li>'+
            '<li name="ko2zh-CHS">韩语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2fr">中文&nbsp; » &nbsp;法语</li>'+
            '<li name="fr2zh-CHS">法语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2de">中文&nbsp; » &nbsp;德语</li>'+
            '<li name="de2zh-CHS">德语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2ru">中文&nbsp; » &nbsp;俄语</li>'+
            '<li name="ru2zh-CHS">俄语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2es">中文&nbsp; » &nbsp;西班牙语</li>'+
            '<li name="es2zh-CHS">西班牙语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2pt">中文&nbsp; » &nbsp;葡萄牙语</li>'+
            '<li name="pt2zh-CHS">葡萄牙语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2it">中文&nbsp; » &nbsp;意大利语</li>'+
            '<li name="it2zh-CHS">意大利语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2vi">中文&nbsp; » &nbsp;越南语</li>'+
            '<li name="vi2zh-CHS">越南语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2id">中文&nbsp; » &nbsp;印尼语</li>'+
            '<li name="id2zh-CHS">印尼语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2ar">中文&nbsp; » &nbsp;阿拉伯语</li>'+
            '<li name="ar2zh-CHS">阿拉伯语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2nl">中文&nbsp; » &nbsp;荷兰语</li>'+
            '<li name="nl2zh-CHS">荷兰语&nbsp; » &nbsp;中文</li>'+
            '<li name="zh-CHS2th">中文&nbsp; » &nbsp;泰语</li>'+
            '<li name="th2zh-CHS">泰语&nbsp; » &nbsp;中文</li>'+
            '</ul>' +
            '</div>' +
            '</div>'
        ).appendTo($(document.body));
    }

    function showBox(mouseX, mouseY) {
        var catBox = document.getElementById('catTranslateBox');
        var selectedRect = window.getSelection().getRangeAt(0).getBoundingClientRect();
        var scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
        if (selectedRect.width) {
            if (getComputedStyle(document.body).position != 'static') {
                catBox.style.top = selectedRect.bottom - document.body.getBoundingClientRect().top + 8 + 'px';
            } else {
                catBox.style.top = selectedRect.bottom + scrollTop + 8 + 'px';
            }
            catBox.style.left = selectedRect.left + selectedRect.width / 2 - 18 + 'px';
        } else {
            catBox.style.top = mouseY - document.body.getBoundingClientRect().y + selectedRect.height + 8 + 'px';
            catBox.style.left = mouseX + selectedRect.width / 2 - 18 + 'px';
        }

        catBox.style.display = 'block';
    }

    function ttranslate() {
        var toLanguage = GM_getValue('toLanguage');
        var url = `http://fanyi.youdao.com/translate?&doctype=json&type=${toLanguage}&i=${gv.encodeText}`;
        var postData = ``;
        getTranslatorRequest(url);
    }

    function parseRes(jsonRes) {
        var explains = '';
        var obj = jsonRes;
        try{
            for(var trans in obj.translateResult){
			  for(var trans2 in obj.translateResult[trans]){
                if(obj.translateResult[trans][trans2].tgt)
                    explains += obj.translateResult[trans][trans2].tgt;
			}
            }
            gv.explains = explains;
            $('.catExplain').text(gv.explains);
        }catch(e){
            $('.catExplain').text('翻译失败');
        }
    }

    function checkClick(e) {
        var path = e.path || e.composedPath();
        if (path.indexOf($('#catTranslateBox').get(0)) > -1) {
            return true;
        } else {
            return false;
        }
    }

    function clearTranslate() {
        $('#catTranslateBox').css('display', '');
        $('.catdropdown').css('display', '');
        $('.catText').empty();
        $('.catExplain').empty();
        try {
            gv.catSource.stop();
        } catch (e) {};
    }

    function playSound(arraybuffer) {
        if(!gv.explains){
            $('.catText').text('No statement');
            setTimeout(function() {
                $('.catText').empty();
            }, 3000);
            return;
        }
        if(!gv.mSpeechSynthesisUtterance){
            gv.mSpeechSynthesisUtterance = new SpeechSynthesisUtterance();
            gv.mSpeechSynthesisUtterance.onstart = function(event) {
                $('.catText').text('playing');
                gv.playSourceStatus = 1;
            };
            gv.mSpeechSynthesisUtterance.onpause = function(event) {
                $('.catText').empty();
                gv.playSourceStatus = 0;
            };
            gv.mSpeechSynthesisUtterance.onresume = function(event) {
                $('.catText').empty();
                gv.playSourceStatus = 0;
            };
            gv.mSpeechSynthesisUtterance.onend = function(event) {
                $('.catText').empty();
                gv.playSourceStatus = 0;
            };
        }
        if(!gv.playSourceStatus){
            gv.playSourceStatus = 1;
            var toLanguage = GM_getValue('toLanguage');
            gv.mSpeechSynthesisUtterance.text = gv.explains;
            gv.mSpeechSynthesisUtterance.lang = toLanguage;
            window.speechSynthesis.speak(gv.mSpeechSynthesisUtterance);
        }
    }

    function postRequest(url, data, headers,fn) {
        GM_xmlhttpRequest({
            method: 'POST',
            url: url,
            data: data,
            headers: headers || {},
            onload: function(res) {
                if (res.status == '200' && res.responseText != '') {
                    if (fn == 'tdetect') {
                        ttranslate(res.responseText);
                    } else if (fn == 'ttranslate') {
                        parseRes(res.responseText);
                    }
                } else if (res.status == '200' && res.finalUrl != url) {
                    console.log('跳转');
                    gv.apiHost = getUrlHost(res.finalUrl);
                    GM_setValue('apiHost', gv.apiHost);
                    postRequest(res.finalUrl, data, fn);
                } else {
                    console.log('发生错误');
                }

            },
        });
    }

    function getTranslatorRequest(url){
        GM_xmlhttpRequest({
            method: 'GET',
            url: url,
            headers: {
            },
            responseType: 'json',
            onload: function(res) {
                parseRes(res.response);
            },
        });
    }
    function getRequest(url,headers) {
        GM_xmlhttpRequest({
            method: 'GET',
            url: url,
            headers: headers || {},
            responseType: 'arraybuffer',
            onload: function(res) {
                playSound(res.response);
            },
        });
    }

    function getUrlHost(url) {
        return url.split('//')[1].split('/')[0];
    }

    function isJSON(str) {
        if (typeof str == 'string') {
            try {
                var obj = JSON.parse(str);
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

    init();