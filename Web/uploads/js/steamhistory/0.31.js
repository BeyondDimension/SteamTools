// ==UserScript==
// @name         steam商店搜索历史记录,足迹
// @icon      	 https://store.steampowered.com/favicon.ico
// @namespace    http://tampermonkey.net/
// @version      0.31
// @description  steam商店搜索输入框添加搜索历史记录功能,游戏商店浏览足迹,支持steam++ 工具客户端导入
// @author       wsz987
// @match        *://store.steampowered.com/*
// @grant        GM_getValue
// @grant        GM_setValue
// @grant        GM_addStyle
// @enable       true
// @supportURL   https://keylol.com/t693504-1-1
// ==/UserScript==

(function () {
    // 自定义历史记录条例上限
    window.HistoryNum = 5

    GM_addStyle(`
.history-table{
    display:none;
    max-width:310px;
    color: #fff;
    box-sizing: border-box;
    background: #316282;
    border: 3px solid lightblue;
    font-size: 15px;
    border-bottom-left-radius: 10px;
    border-bottom-right-radius: 10px;
    list-style-type: none;
}
.history-table a,
.history-table .history-item{
    padding: 4px 6px;
    box-sizing: border-box;
    cursor: pointer;
    display: block;
    overflow:hidden;
    text-overflow:ellipsis;
}
.history-table .history-item{
    justify-content: center;
    align-items: center;
    display: flex;
    word-break:break-all;
}
.history-table a,
.history-table li.history-item:not(:last-child){
    border-bottom: 2px dashed lightblue;
}
.history-table a{
    color:lightblue;
    font-weight: bold;
}
.last-li{
    justify-content: center;
    align-items: center;
    display: flex;
    padding:3px 0;
}
.historyClear{
    cursor: pointer;
}
`)
    initHistory()
    onlistener()
    $J('.apphub_AppName').text() ? GM_setValue('footName', $J('.apphub_AppName').text()) : false
})();

function initHistory() {
    console.log('render')
    $J('.history-table').remove()
    !GM_getValue('historyData') ? GM_setValue('historyData', []) : console.log('初始化')
    // DOM render
    let li_DOM = ''
    for (let val of GM_getValue('historyData')) {
        li_DOM += `<li class='history-item' onclick="window.open('https://store.steampowered.com/search/?term=${val}','_self')">${val}</li>`
    }
    let formCtx = $J("#store_search_link").parents('form').context.referrer
    let foot = `<li style="text-align:center;padding:3px 0">足迹:</br> <a href='${formCtx}'> ${formCtx.split('/')[5] ? GM_getValue('footName') : '暂无'}</a></li>`
    let table_DOM = `<ul class='history-table'>${foot}${li_DOM}${GM_getValue('historyData').length===0 ? '<li class="last-li">暂无搜索记录...</li>':'<li class="last-li historyClear"><svg t="1615284325596" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="2293" width="16" height="16"><path d="M517.59 21.609c-100.299 0-181.703 79.514-185.167 179.34H98.603c-25.979 0-47.235 21.099-47.235 47.236 0 25.98 21.099 47.237 47.236 47.237h52.117v528.416c0 99.196 67.233 180.285 150.37 180.285h423.55c82.98 0 150.37-80.616 150.37-180.285V295.737h47.236c25.98 0 47.236-21.1 47.236-47.237 0-25.98-21.099-47.236-47.236-47.236H702.441C699.449 101.124 617.888 21.61 517.59 21.61z m-96.677 179.34c3.464-51.172 45.19-90.85 96.834-90.85s93.37 39.835 96.362 90.85H420.913z m-119.98 714.842c-29.444 0-61.88-37.789-61.88-91.953V295.737h547.311V824.31c0 54.007-32.436 91.954-61.88 91.954H300.933v-0.473z m0 0" p-id="2294" fill="#ffffff"></path><path d="M364.387 802.267c21.57 0 39.363-21.571 39.363-48.653V476.022c0-27.082-17.635-48.654-39.363-48.654-21.572 0-39.364 21.572-39.364 48.654v277.592c0 26.924 17.32 48.653 39.364 48.653z m142.496 0c21.571 0 39.363-21.571 39.363-48.653V476.022c0-27.082-17.635-48.654-39.363-48.654-21.571 0-39.364 21.572-39.364 48.654v277.592c0 26.924 17.793 48.653 39.364 48.653z m149.896 0c21.571 0 39.364-21.571 39.364-48.653V476.022c0-27.082-17.635-48.654-39.364-48.654-21.571 0-39.363 21.572-39.363 48.654v277.592c0 26.924 17.162 48.653 39.363 48.653z m0 0" p-id="2295" fill="#ffffff"></path></svg>清空搜索记录.</li>'}</ul>`
    $J("#store_nav_search_term, input#term") ? $J(".searchbox, .searchbar_left").append(table_DOM) : ''
    // DOM 动态style
    $J("#store_nav_search_term, input#term").click(el => {
        $J(`#${el.target.id} ~ .history-table`)
            .css('width', el.target.id === 'store_nav_search_term' ?
                $J('.searchbox')[0].offsetWidth - 33 : $J("#" + el.target.id)[0].offsetWidth + 'px')
            .css('display', 'block')
    }).blur(el => {
        setTimeout(function () {
            $J(`#${el.target.id} ~ .history-table`).css('display', 'none')
        }, 250)
    });
}

function onlistener(){
    // onClicklistener
    $J('#store_search_link, .searchbar_left button').click(() => {
        setHistoryData()
    })
    // onEnterlistener
    $J('#store_nav_search_term, input#term').keydown(function (event) {
        if (event.keyCode == 13) {
            setHistoryData()
        }
    })
    // 全局事件委托监测动态DOM
    $J('body').on("click", '#term_options > ul > li', () => {
        setTimeout(function () {
            setHistoryData()
        }, 250)
    })
    // Clear Data
    $J('body').on("click", '.historyClear', () => {
        GM_setValue('historyData', [])
        GM_setValue('footName', '')
        initHistory()
    })
}
// Data storage, reload DOM
function setHistoryData() {
    let input_val = $J("#store_nav_search_term")[0].value.toString().trim() || $J("input#term")[0].value.toString().trim()
    if (input_val === '') return
    let oldVal = GM_getValue('historyData')
    oldVal.unshift(input_val)
    oldVal = Array.from(new Set(oldVal))
    oldVal.length > window.HistoryNum ? oldVal.pop() : console.log('Data storage')
    GM_setValue('historyData', oldVal)
    initHistory()
}