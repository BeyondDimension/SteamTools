// ==UserScript==
// @name        Steam历史最低价格查询
// @namespace   SteamHistoryLowestPrice
// @description 在steam商店页显示当前app历史最低价格及进包次数(此脚本需要勾选Steam商店加速才会生效)
// @include      https://store.steampowered.com/app/*
// @include      https://store.steampowered.com/bundle/*
// @include      https://store.steampowered.com/sub/*
// @author      软妹币玩家、byzod
// @license     GPL version 3 or any later version
// @version     1.1
// @grant       GM_xmlhttpRequest
// @enable      true
// jshint esversion:6
// ==/UserScript==

// 显示样式
// 0 = 显示在购买按钮上面
// 1 = 显示在购买信息框上面
const INFO_STYLE = 1;

// 货币区域覆盖，两个字母的国家代号,大小写均可
// 空字符（""）代表不覆盖，使用steam的cookie中steamCountry的值
// 见 https://zh.wikipedia.org/wiki/ISO_3166-1 或 https://en.wikipedia.org/wiki/ISO_3166-1
// 常用 美国USD:"us", 中国CNY: "cn", 英国GBP: "uk", 日本JPY: "jp", 俄国RUS: "ru"
const CC_OVERRIDE = "";

// 货币符号
const CURRENCY_SYMBOLS = {
    'AED': 'DH',
    'AUD': 'A$',
    'BRL': 'R$',
    'CAD': 'CDN$',
    'CHF': 'CHF',
    'CLP': 'CLP$',
    'CNY': '¥',  // Chines Yuan
    'COP': 'COL$',
    'CRC': '₡',    // Costa Rican Colón
    'EUR': '€',    // Euro
    'GBP': '£',    // British Pound Sterling
    'HKD': 'HK$',
    'IDR': 'Rp',
    'ILS': '₪',    // Israeli New Sheqel
    'INR': '₹',    // Indian Rupee
    'JPY': '¥',    // Japanese Yen
    'KRW': '₩',    // South Korean Won
    'MXN': 'Mex$',
    'MYR': 'RM',
    'NGN': '₦',    // Nigerian Naira
    'NOK': 'kr',
    'NZD': 'NZ$',
    'PEN': 'S/.',
    'PHP': '₱',    // Philippine Peso
    'PLN': 'zł',   // Polish Zloty
    'PYG': '₲',    // Paraguayan Guarani
    'RUB': 'pуб',
    'SAR': 'SR',
    'SGD': 'S$',
    'THB': '฿',    // Thai Baht
    'TRY': 'TL',
    'TWD': 'NT$',
    'UAH': '₴',    // Ukrainian Hryvnia
    'USD': '$',    // US Dollar
    'VND': '₫',    // Vietnamese Dong
    'ZAR': 'R ',
};

// 查询历史低价包括的商店
const STORES = [
    "steam",
    // "amazonus",
    // "impulse",
    // "gamersgate",
    // "direct2drive",
    // "origin",
    // "uplay",
    // "indiegalastore",
    // "gamesplanet",
    // "indiegamestand",
    // "gog",
    // "nuuvem",
    // "dlgamer",
    // "humblestore",
    // "squenix",
    // "bundlestars",
    // "fireflower",
    // "humblewidgets",
    // "newegg",
    // "coinplay",
    // "wingamestore",
    // "macgamestore",
    // "gamebillet",
    // "silagames",
    // "itchio",
    // "gamejolt",
    // "paradox"
];


// 在app页和愿望单页显示史低价格
let urlMatch = location.href.match(/(app|sub|bundle)\/(\d+)/);
let appId = "";
let type = "";
let subIds = [];
let bundleids = [];
if (urlMatch && urlMatch.length == 3) {
    type = urlMatch[1]
    appId = urlMatch[2];
}

// console.log('[史低]: ' + type + ' : ' + appId + ', requesting data info...'); // DEBUG

// 获取subs
document.querySelectorAll("input[name=subid]")
    .forEach(sub => subIds.push(sub.value));
// 获取bundles
document.querySelectorAll("input[name=bundleid]")
    .forEach(sub => bundleids.push(sub.value));

let cc = "cn";
if (CC_OVERRIDE.length > 0) {
    // 使用覆盖的货币区域
    cc = CC_OVERRIDE;
} else {
    // 使用默认的的货币区域
    let ccMatch = document.cookie.match(/steamCountry=([a-z]{2})/i);
    if (ccMatch !== null && ccMatch.length == 2) {
        cc = ccMatch[1];
    }
}

AddLowestPriceTag(appId, type, subIds, bundleids, STORES.join(","), cc, location.protocol);

// 在商店页添加史低信息
async function AddLowestPriceTag(appId, type = "app", subIds = [], bundleids = [], stores = "steam", cc = "cn", protocol = "https") {
    // 史低信息容器们
    let lowestPriceNodes = {};

    // 统计subid
    let findSubIds = [];
    if (type == "bundle") {
        // bundle就一个, 视作subid
        findSubIds.push(appId);
    } else if (type == "app" || type == "sub") {
        // app/sub/bundle 可能有好多
        findSubIds = subIds.slice();
        if (bundleids.length > 0) {
            findSubIds.push.apply(findSubIds, bundleids);
        }
    }

    // 寻找每一个subid的购买按钮，生成史低信息容器们
    findSubIds.forEach(subId => {
        let gameWrapper = null;
        try {
            gameWrapper = document.querySelector('.game_area_purchase_game input[value="' + subId + '"]');
            switch (INFO_STYLE) {
                case 0:
                    gameWrapper = gameWrapper.parentNode.parentNode.querySelector('.game_purchase_action');
                    break;
                case 1:
                    gameWrapper = gameWrapper.parentNode.parentNode;
                    break;
            }
        } catch (ex) {
            gameWrapper = null;
        }
        if (gameWrapper) {
            let lowestInfo = document.createElement("div");
            lowestInfo.className = "game_lowest_price";
            lowestInfo.innerText = "正在读取历史最低价格...";
            switch (INFO_STYLE) {
                case 0:
                    gameWrapper.prepend(lowestInfo);
                    break;
                case 1:
                    gameWrapper.append(lowestInfo);
                    break;
            }
            lowestPriceNodes[subId] = lowestInfo;
        }
    });

    // 获取sub们的数据
    let data = null;
    try {
        data = await GettingSteamDBAppInfo(appId, type, subIds, stores, cc, protocol);
        if ((typeof data == 'string'))
            data = JSON.parse(data);
        if (data.result == 'success') {
            data = data.data;
        } else {
            console.log('[史低]: ' + err);
        }
    } catch (err) {
        console.log('[史低]: ' + err);
    }

    // console.log('[史低]: app ' + appId + ' data : %o', data); // DEBUG

    // 解析data
    let appInfos = [];
    let metaInfo = data ? data[".meta"] : null;
    // 如果是bundle， 除了.meta外只有一个bundle/xxx，否则是一大堆xxx
    if (type == "bundle") {
        data = data.data;
        appInfos.push({ Id: appId, Info: data["bundle/" + appId] });
    } else if (type == "app" || type == "sub") {
        data = data.data;
        for (let key in data) {
            let appid = key.replace(new RegExp('(app|sub|bundle)/'), "");
            if (!isNaN(appid)) {
                appInfos.push({ Id: appid, Info: data[key] });
            }
        }
    }
    // console.log('[史低]: app ' + appId + ' metaInfo: %o', metaInfo); // DEBUG
    // console.log('[史低]: app ' + appId + ' appInfos: %o', appInfos); // DEBUG

    // 如果查到info，塞到购买按钮上面去
    if (metaInfo && appInfos.length > 0) {
        // 获取整体信息
        let currencySymbol = metaInfo.currency in CURRENCY_SYMBOLS
            ? CURRENCY_SYMBOLS[metaInfo.currency]
            : metaInfo.currency;

        // 为每一个sub或bundle添加史低
        appInfos.forEach(app => {
            let lowestInfo = lowestPriceNodes[app.Id];

            if (lowestInfo) {
                lowestInfo.innerHTML =
                    // 历史最低
                    '历史最低价是&nbsp;'
                    + '<span style="cursor:help;text-decoration:underline;" title="' + app.Info.lowest.recorded_formatted + '">'
                    + new Date(app.Info.lowest.recorded * 1e3).toLocaleDateString()
                    + '</span>&nbsp;时在&nbsp;'
                    + '<a target="_blank" href="' + app.Info.lowest.url + '"> '
                    + app.Info.lowest.store
                    + '</a>&nbsp;中的&nbsp;'
                    + '<span class="discount_pct">-' + app.Info.lowest.cut + '%</span>'
                    + '<a target="_blank" title="查看价格历史" href="' + app.Info.urls.history + '"> '
                    + currencySymbol + ' ' + app.Info.lowest.price
                    + '</a>'

                    // 第二行
                    + '<br />'

                    // 进包次数
                    + '进包&nbsp;<a target="_blank" title="查看进包信息"  href="' + app.Info.urls.bundles + '"'
                    + (app.Info.bundles && app.Info.bundles.live.length > 0
                        ? ' style="color:#0d0" title="' + app.Info.bundles.live.length + ' 个正在销售的慈善包"'
                        : '')
                    + '> '
                    + app.Info.bundles.count
                    + ' </a>&nbsp;次，'

                    // 当前最低
                    + (app.Info.price.price <= app.Info.lowest.price
                        ? '<span class="game_purchase_discount_countdown">当前为历史最低价</span>，'
                        : '<span>当前最低价是</span>')
                    + '在&nbsp;'
                    + '<a target="_blank" href="' + app.Info.price.url + '"> '
                    + app.Info.price.store
                    + '</a>&nbsp;中的&nbsp;'
                    + '<span class="discount_pct">-' + app.Info.price.cut + '%</span>'
                    + '<a target="_blank" title="查看价格信息"  href="' + app.Info.urls.info + '"> '
                    + currencySymbol + ' ' + app.Info.price.price
                    + '</a>';
            }
        });
    } else {
        // metaInfo为空，或者appInfos无内容
        console.log('[史低]: get lowest price failed, data = %o', data);
        for (let id in lowestPriceNodes) {
            lowestPriceNodes[id].innerHTML = "";
        }
    }

    // 返回史低info
    return Promise.resolve(lowestPriceNodes);
}

// 获取史低信息
async function GettingSteamDBAppInfo(appId, type = "app", subIds = [], stores = "steam", cc = "cn", protocol = "https") {
    let requestPromise = null;
    let bundleId = "";

    if (type == "bundle") {
        bundleId = appId;
    } else if (type == "app" || type == "sub") {
        bundleId = bundleids.join(',');
    }
    if (!isNaN(appId) && parseInt(appId) > 0) {
        let requestUrl = protocol + "//esapi.isthereanydeal.com/v01/prices/?"
            + "bundleids=" + bundleId
            + "&subids=" + subIds.join(',')
            + "&appids=" + appId
            + "&stores=" + stores
            + "&cc=" + cc
            + "&coupon=true";
        // console.log('[史低]: requestUrl: ' + requestUrl); // DEBUG
        requestPromise = new Promise((resolve, reject) => {
            GM_xmlhttpRequest({
                method: "GET",
                url: requestUrl,
                onload: function (response) {
                    resolve(response.response);
                },
                onerror: function (error) {
                    reject(error);
                }
            });
        });
    } else {
        requestPromise = Promise.reject("Invalid appid");
    }

    return requestPromise;
}