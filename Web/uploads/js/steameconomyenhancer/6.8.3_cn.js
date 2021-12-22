// ==UserScript==
// @name        Steam Economy Enhancer
// @icon        https://upload.wikimedia.org/wikipedia/commons/8/83/Steam_icon_logo.svg
// @namespace   https://github.com/Nuklon
// @author      Nuklon
// @license     MIT
// @version     6.8.3_Cn
// @description 增强 Steam 库存和 Steam 市场功能，实现批量出售饰品、卡片功能，并且能自动计算价格【此功能需要先开启Steam社区加速】
// @include     *://steamcommunity.com/id/*/inventory*
// @include     *://steamcommunity.com/profiles/*/inventory*
// @include     *://steamcommunity.com/market*
// @include     *://steamcommunity.com/tradeoffer*
// @require     https://cdn.jsdelivr.net/npm/jquery@3.3.1/dist/jquery.min.js
// @require     http://cdn.bootcss.com/jqueryui/1.12.1/jquery-ui.min.js
// @require     https://cdn.jsdelivr.net/gh/kapetan/jquery-observe/jquery-observe.js
// @require     https://cdn.jsdelivr.net/npm/paginationjs@2.1.2/dist/pagination.min.js
// @require     https://cdn.jsdelivr.net/npm/async@2.6.0/dist/async.min.js
// @require     https://cdn.jsdelivr.net/npm/localforage@1.7.1/dist/localforage.min.js
// @require     https://cdn.jsdelivr.net/npm/luxon@1.24.1/build/global/luxon.min.js
// @require     https://cdn.jsdelivr.net/npm/list.js@1.5.0/dist/list.min.js
// @require     https://cdn.jsdelivr.net/npm/checkboxes.js@1.2.2/dist/jquery.checkboxes-1.2.2.min.js
// @grant       unsafeWindow
// @grant       GM_addStyle
// @homepageURL https://steamcn.com/t311996-1-1
// @supportURL  https://steamcn.com/t311996-1-1
// @downloadURL https://raw.githubusercontent.com/Sneer-Cat/Steam-Economy-Enhancer/master/code.user.js
// @updateURL   https://raw.githubusercontent.com/Sneer-Cat/Steam-Economy-Enhancer/master/code.user.js
// ==/UserScript==
// jQuery is already added by Steam, force no conflict mode.
(function($, async) {
    $.noConflict(true);

    var DateTime = luxon.DateTime;

    const STEAM_INVENTORY_ID = 753;

    const PAGE_MARKET = 0;
    const PAGE_MARKET_LISTING = 1;
    const PAGE_TRADEOFFER = 2;
    const PAGE_INVENTORY = 3;

    const COLOR_ERROR = '#8A4243';
    const COLOR_SUCCESS = '#407736';
    const COLOR_PENDING = '#908F44';
    const COLOR_PRICE_FAIR = '#496424';
    const COLOR_PRICE_CHEAP = '#837433';
    const COLOR_PRICE_EXPENSIVE = '#813030';
    const COLOR_PRICE_NOT_CHECKED = '#26566c';

    const ERROR_SUCCESS = null;
    const ERROR_FAILED = 1;
    const ERROR_DATA = 2;

    var marketLists = [];
    var totalNumberOfProcessedQueueItems = 0;
    var totalNumberOfQueuedItems = 0;
    var totalPriceWithFeesOnMarket = 0;
    var totalPriceWithoutFeesOnMarket = 0;
    var totalScrap = 0;

    var spinnerBlock =
        '<div class="spinner"><div class="rect1"></div>&nbsp;<div class="rect2"></div>&nbsp;<div class="rect3"></div>&nbsp;<div class="rect4"></div>&nbsp;<div class="rect5"></div>&nbsp;</div>';
    var numberOfFailedRequests = 0;

    var enableConsoleLog = false;

    var isLoggedIn = typeof unsafeWindow.g_rgWalletInfo !== 'undefined' && unsafeWindow.g_rgWalletInfo != null || (typeof unsafeWindow.g_bLoggedIn !== 'undefined' && unsafeWindow.g_bLoggedIn);

    var currentPage = window.location.href.includes('.com/market') ?
        (window.location.href.includes('market/listings') ?
            PAGE_MARKET_LISTING :
            PAGE_MARKET) :
        (window.location.href.includes('.com/tradeoffer') ?
            PAGE_TRADEOFFER :
            PAGE_INVENTORY);

    var market = new SteamMarket(unsafeWindow.g_rgAppContextData,
        typeof unsafeWindow.g_strInventoryLoadURL !== 'undefined' && unsafeWindow.g_strInventoryLoadURL != null ?
        unsafeWindow.g_strInventoryLoadURL :
        location.protocol + '//steamcommunity.com/my/inventory/json/',
        isLoggedIn ? unsafeWindow.g_rgWalletInfo : undefined);

    var currencyId =
        isLoggedIn &&
        market != null &&
        market.walletInfo != null &&
        market.walletInfo.wallet_currency != null ?
        market.walletInfo.wallet_currency :
        3;

    var currencySymbol = unsafeWindow.GetCurrencySymbol(unsafeWindow.GetCurrencyCode(currencyId));

    function SteamMarket(appContext, inventoryUrl, walletInfo) {
        this.appContext = appContext;
        this.inventoryUrl = inventoryUrl;
        this.walletInfo = walletInfo;
        this.inventoryUrlBase = inventoryUrl.replace('/inventory/json', '');
        if (!this.inventoryUrlBase.endsWith('/'))
            this.inventoryUrlBase += '/';
    }

    //#region Settings
    const SETTING_MIN_NORMAL_PRICE = 'SETTING_MIN_NORMAL_PRICE';
    const SETTING_MAX_NORMAL_PRICE = 'SETTING_MAX_NORMAL_PRICE';
    const SETTING_MIN_FOIL_PRICE = 'SETTING_MIN_FOIL_PRICE';
    const SETTING_MAX_FOIL_PRICE = 'SETTING_MAX_FOIL_PRICE';
    const SETTING_MIN_MISC_PRICE = 'SETTING_MIN_MISC_PRICE';
    const SETTING_MAX_MISC_PRICE = 'SETTING_MAX_MISC_PRICE';
    const SETTING_PRICE_OFFSET = 'SETTING_PRICE_OFFSET';
    const SETTING_PRICE_MIN_CHECK_PRICE = 'SETTING_PRICE_MIN_CHECK_PRICE';
    const SETTING_PRICE_ALGORITHM = 'SETTING_PRICE_ALGORITHM';
    const SETTING_PRICE_IGNORE_LOWEST_Q = 'SETTING_PRICE_IGNORE_LOWEST_Q';
    const SETTING_PRICE_HISTORY_HOURS = 'SETTING_PRICE_HISTORY_HOURS';
    const SETTING_INVENTORY_PRICE_LABELS = 'SETTING_INVENTORY_PRICE_LABELS';
    const SETTING_TRADEOFFER_PRICE_LABELS = 'SETTING_TRADEOFFER_PRICE_LABELS';
    const SETTING_LAST_CACHE = 'SETTING_LAST_CACHE';
    const SETTING_RELIST_AUTOMATICALLY = 'SETTING_RELIST_AUTOMATICALLY';
    const SETTING_MARKET_PAGE_COUNT = 'SETTING_MARKET_PAGE_COUNT';
    const SETTING_INVENTORY_PRICES = 'SETTING_INVENTORY_PRICES';

    var settingDefaults = {
        SETTING_MIN_NORMAL_PRICE: 0.05,
        SETTING_MAX_NORMAL_PRICE: 2.50,
        SETTING_MIN_FOIL_PRICE: 0.15,
        SETTING_MAX_FOIL_PRICE: 10,
        SETTING_MIN_MISC_PRICE: 0.05,
        SETTING_MAX_MISC_PRICE: 10,
        SETTING_PRICE_OFFSET: 0.00,
        SETTING_PRICE_MIN_CHECK_PRICE: 0.00,
        SETTING_PRICE_ALGORITHM: 1,
        SETTING_PRICE_IGNORE_LOWEST_Q: 1,
        SETTING_PRICE_HISTORY_HOURS: 12,
        SETTING_INVENTORY_PRICE_LABELS: 1,
        SETTING_TRADEOFFER_PRICE_LABELS: 1,
        SETTING_LAST_CACHE: 0,
        SETTING_RELIST_AUTOMATICALLY: 0,
        SETTING_MARKET_PAGE_COUNT: 100
    };

    GM_addStyle('.inventory_iteminfo .market_commodity_orders_table th {min-width: 69px; padding: 4px} .inventory_iteminfo .market_commodity_orders_table td {min-width: initial} @media screen and (max-width: 910px) {html.responsive .view_inventory_logo {max-height: unset !important;}}');

    function getSettingWithDefault(name) {
        return getLocalStorageItem(name) || (name in settingDefaults ? settingDefaults[name] : null);
    }

    function setSetting(name, value) {
        setLocalStorageItem(name, value);
    }
    //#endregion

    //#region Storage

    var storagePersistent = localforage.createInstance({
        name: 'see_persistent'
    });

    var storageSession;

    var currentUrl = new URL(window.location.href);
    var noCache = currentUrl.searchParams.get('no-cache') != null;

    // This does not work the same as the 'normal' session storage because opening a new browser session/tab will clear the cache.
    // For this reason, a rolling cache is used.
    if (getSessionStorageItem('SESSION') == null || noCache) {
        var lastCache = getSettingWithDefault(SETTING_LAST_CACHE);
        if (lastCache > 5)
            lastCache = 0;

        setSetting(SETTING_LAST_CACHE, lastCache + 1);

        storageSession = localforage.createInstance({
            name: 'see_session_' + lastCache
        });

        storageSession.clear(); // Clear any previous data.
        setSessionStorageItem('SESSION', lastCache);
    } else {
        storageSession = localforage.createInstance({
            name: 'see_session_' + getSessionStorageItem('SESSION')
        });
    }

    function getLocalStorageItem(name) {
        try {
            return localStorage.getItem(name);
        } catch (e) {
            return null;
        }
    }

    function setLocalStorageItem(name, value) {
        try {
            localStorage.setItem(name, value);
            return true;
        } catch (e) {
            logConsole('无法设置localStorage内容，名称：' + name + '，原因：' + e + '。')
            return false;
        }
    }

    function getSessionStorageItem(name) {
        try {
            return sessionStorage.getItem(name);
        } catch (e) {
            return null;
        }
    }

    function setSessionStorageItem(name, value) {
        try {
            sessionStorage.setItem(name, value);
            return true;
        } catch (e) {
            logConsole('无法设置sessionStorage内容，名称：' + name + '，原因：' + e + '。')
            return false;
        }
    }
    //#endregion

    //#region Price helpers
    function getPriceInformationFromItem(item) {
        var isTradingCard = getIsTradingCard(item);
        var isFoilTradingCard = getIsFoilTradingCard(item);
        return getPriceInformation(isTradingCard, isFoilTradingCard);
    }

    function getPriceInformation(isTradingCard, isFoilTradingCard) {
        var maxPrice = 0;
        var minPrice = 0;

        if (!isTradingCard) {
            maxPrice = getSettingWithDefault(SETTING_MAX_MISC_PRICE);
            minPrice = getSettingWithDefault(SETTING_MIN_MISC_PRICE);
        } else {
            maxPrice = isFoilTradingCard ?
                getSettingWithDefault(SETTING_MAX_FOIL_PRICE) :
                getSettingWithDefault(SETTING_MAX_NORMAL_PRICE);
            minPrice = isFoilTradingCard ?
                getSettingWithDefault(SETTING_MIN_FOIL_PRICE) :
                getSettingWithDefault(SETTING_MIN_NORMAL_PRICE);
        }

        maxPrice = maxPrice * 100.0;
        minPrice = minPrice * 100.0;

        var maxPriceBeforeFees = market.getPriceBeforeFees(maxPrice);
        var minPriceBeforeFees = market.getPriceBeforeFees(minPrice);

        return {
            maxPrice,
            minPrice,
            maxPriceBeforeFees,
            minPriceBeforeFees
        };
    }

    // Calculates the average history price, before the fee.
    function calculateAverageHistoryPriceBeforeFees(history) {
        var highest = 0;
        var total = 0;

        if (history != null) {
            // Highest average price in the last xx hours.
            var timeAgo = Date.now() - (getSettingWithDefault(SETTING_PRICE_HISTORY_HOURS) * 60 * 60 * 1000);

            history.forEach(function(historyItem) {
                var d = new Date(historyItem[0]);
                if (d.getTime() > timeAgo) {
                    highest += historyItem[1] * historyItem[2];
                    total += historyItem[2];
                }
            });
        }

        if (total == 0)
            return 0;

        highest = Math.floor(highest / total);
        return market.getPriceBeforeFees(highest);
    }

    // Calculates the listing price, before the fee.
    function calculateListingPriceBeforeFees(histogram) {
        if (typeof histogram === 'undefined' ||
            histogram == null ||
            histogram.lowest_sell_order == null ||
            histogram.sell_order_graph == null)
            return 0;

        var listingPrice = market.getPriceBeforeFees(histogram.lowest_sell_order);

        var shouldIgnoreLowestListingOnLowQuantity = getSettingWithDefault(SETTING_PRICE_IGNORE_LOWEST_Q) == 1;

        if (shouldIgnoreLowestListingOnLowQuantity && histogram.sell_order_graph.length >= 2) {
            var listingPrice2ndLowest = market.getPriceBeforeFees(histogram.sell_order_graph[1][0] * 100);

            if (listingPrice2ndLowest > listingPrice) {
                var numberOfListingsLowest = histogram.sell_order_graph[0][1];
                var numberOfListings2ndLowest = histogram.sell_order_graph[1][1];

                var percentageLower = (100 * (numberOfListingsLowest / numberOfListings2ndLowest));

                // The percentage should change based on the quantity (for example, 1200 listings vs 5, or 1 vs 25).
                if (numberOfListings2ndLowest >= 1000 && percentageLower <= 5) {
                    listingPrice = listingPrice2ndLowest;
                } else if (numberOfListings2ndLowest < 1000 && percentageLower <= 10) {
                    listingPrice = listingPrice2ndLowest;
                } else if (numberOfListings2ndLowest < 100 && percentageLower <= 15) {
                    listingPrice = listingPrice2ndLowest;
                } else if (numberOfListings2ndLowest < 50 && percentageLower <= 20) {
                    listingPrice = listingPrice2ndLowest;
                } else if (numberOfListings2ndLowest < 25 && percentageLower <= 25) {
                    listingPrice = listingPrice2ndLowest;
                } else if (numberOfListings2ndLowest < 10 && percentageLower <= 30) {
                    listingPrice = listingPrice2ndLowest;
                }
            }
        }

        return listingPrice;
    }

    function calculateBuyOrderPriceBeforeFees(histogram) {
        if (typeof histogram === 'undefined')
            return 0;

        return market.getPriceBeforeFees(histogram.highest_buy_order);
    }

    // Calculate the sell price based on the history and listings.
    // applyOffset specifies whether the price offset should be applied when the listings are used to determine the price.
    function calculateSellPriceBeforeFees(history, histogram, applyOffset, minPriceBeforeFees, maxPriceBeforeFees) {
        var historyPrice = calculateAverageHistoryPriceBeforeFees(history);
        var listingPrice = calculateListingPriceBeforeFees(histogram);
        var buyPrice = calculateBuyOrderPriceBeforeFees(histogram);

        var shouldUseAverage = getSettingWithDefault(SETTING_PRICE_ALGORITHM) == 1;
        var shouldUseBuyOrder = getSettingWithDefault(SETTING_PRICE_ALGORITHM) == 3;

        // If the highest average price is lower than the first listing, return the offset + that listing.
        // Otherwise, use the highest average price instead.
        var calculatedPrice = 0;
        if (shouldUseBuyOrder && buyPrice !== -2) {
            calculatedPrice = buyPrice;
        } else if (historyPrice < listingPrice || !shouldUseAverage) {
            calculatedPrice = listingPrice;
        } else {
            calculatedPrice = historyPrice;
        }

        var changedToMax = false;
        // List for the maximum price if there are no listings yet.
        if (calculatedPrice == 0) {
            calculatedPrice = maxPriceBeforeFees;
            changedToMax = true;
        }


        // Apply the offset to the calculated price, but only if the price wasn't changed to the max (as otherwise it's impossible to list for this price).
        if (!changedToMax && applyOffset) {
            calculatedPrice = calculatedPrice + (getSettingWithDefault(SETTING_PRICE_OFFSET) * 100);
        }


        // Keep our minimum and maximum in mind.
        calculatedPrice = clamp(calculatedPrice, minPriceBeforeFees, maxPriceBeforeFees);


        // In case there's a buy order higher than the calculated price.
        if (typeof histogram !== 'undefined' && histogram != null && histogram.highest_buy_order != null) {
            var buyOrderPrice = market.getPriceBeforeFees(histogram.highest_buy_order);
            if (buyOrderPrice > calculatedPrice)
                calculatedPrice = buyOrderPrice;
        }

        return calculatedPrice;
    }
    //#endregion

    //#region Integer helpers
    function getRandomInt(min, max) {
        return Math.floor(Math.random() * (max - min + 1)) + min;
    }

    function getNumberOfDigits(x) {
        return (Math.log10((x ^ (x >> 31)) - (x >> 31)) | 0) + 1;
    }

    function padLeftZero(str, max) {
        str = str.toString();
        return str.length < max ? padLeftZero("0" + str, max) : str;
    }

    function replaceNonNumbers(str) {
        return str.replace(/\D/g, '');
    }
    //#endregion

    //#region Steam Market

    // Sell an item with a price in cents.
    // Price is before fees.
    SteamMarket.prototype.sellItem = function(item, price, callback /*err, data*/ ) {
        var sessionId = readCookie('sessionid');
        var itemId = item.assetid || item.id;
        $.ajax({
            type: "POST",
            url: 'https://steamcommunity.com/market/sellitem/',
            data: {
                sessionid: sessionId,
                appid: item.appid,
                contextid: item.contextid,
                assetid: itemId,
                amount: 1,
                price: price
            },
            success: function(data) {
                if (data.success === false && isRetryMessage(data.message)) {
                    callback(ERROR_FAILED, data);
                } else {
                    callback(ERROR_SUCCESS, data);
                }
            },
            error: function(data) {
                return callback(ERROR_FAILED, data);
            },
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            dataType: 'json'
        });
    };

    // Removes an item.
    // Item is the unique item id.
    SteamMarket.prototype.removeListing = function(item, callback /*err, data*/ ) {
        var sessionId = readCookie('sessionid');
        $.ajax({
            type: "POST",
            url: window.location.protocol + '//steamcommunity.com/market/removelisting/' + item,
            data: {
                sessionid: sessionId
            },
            success: function(data) {
                callback(ERROR_SUCCESS, data);
            },
            error: function() {
                return callback(ERROR_FAILED);
            },
            crossDomain: true,
            xhrFields: {
                withCredentials: true
            },
            dataType: 'json'
        });
    };

    // Get the price history for an item.
    //
    // PriceHistory is an array of prices in the form [data, price, number sold].
    // Example: [["Fri, 19 Jul 2013 01:00:00 +0000",7.30050206184,362]]
    // Prices are ordered by oldest to most recent.
    // Price is inclusive of fees.
    SteamMarket.prototype.getPriceHistory = function(item, cache, callback) {
        try {
            var market_name = getMarketHashName(item);
            if (market_name == null) {
                callback(ERROR_FAILED);
                return;
            }

            var appid = item.appid;

            if (cache) {
                var storage_hash = 'pricehistory_' + appid + '+' + market_name;

                storageSession.getItem(storage_hash)
                    .then(function(value) {
                        if (value != null)
                            callback(ERROR_SUCCESS, value, true);
                        else
                            market.getCurrentPriceHistory(appid, market_name, callback);
                    })
                    .catch(function(error) {
                        market.getCurrentPriceHistory(appid, market_name, callback);
                    });
            } else
                market.getCurrentPriceHistory(appid, market_name, callback);
        } catch (e) {
            return callback(ERROR_FAILED);
        }
    };

    SteamMarket.prototype.getGooValue = function(item, callback) {
        try {
            for (var i = 0; i < item.owner_actions.length; i++) {
                var action = item.owner_actions[i];
                if ( !action.link || !action.name ) {
                    continue;
                }
                if (action.link.match(/^javascript:GetGooValue/)) {
                    var item_data = action.link.split(',');
                    var appid = item_data[2].trim();
                    var item_type = item_data[3].trim();
                    var border_color = item_data[4].trim();
                    $.ajax({
                        type: "GET",
                        url: window.location.protocol+'//steamcommunity.com/auction/ajaxgetgoovalueforitemtype/',
                        data: {
                            appid: appid,
                            item_type: item_type,
                            border_color: border_color
                        },
                        success: function(data) {
                            callback(ERROR_SUCCESS, data);
                        },
                        error: function(data) {
                            return callback(ERROR_FAILED, data);
                        },
                        crossDomain: true,
                        xhrFields: {
                            withCredentials: true
                        },
                        dataType: 'json'
                    });
                }
            }
        } catch (e) {
            return callback(ERROR_FAILED);
        }
        //http://steamcommunity.com/auction/ajaxgetgoovalueforitemtype/?appid=582980&item_type=18&border_color=0
        // OR
        //http://steamcommunity.com/my/ajaxgetgoovalue/?sessionid=xyz&appid=535690&assetid=4830605461&contextid=6
        //sessionid=xyz
        //appid = 535690
        //assetid = 4830605461
        //contextid = 6
    }

    // Grinds the item into gems.
    SteamMarket.prototype.grindIntoGoo = function(item, callback) {
        try {
            var sessionId = readCookie('sessionid');
            $.ajax({
                type: "POST",
                url: this.inventoryUrlBase + 'ajaxgrindintogoo/',
                data: {
                    sessionid: sessionId,
                    appid: item.market_fee_app,
                    assetid: item.assetid,
                    contextid: item.contextid,
                    goo_value_expected: item.goo_value_expected
                },
                success: function(data) {
                    callback(ERROR_SUCCESS, data);
                },
                error: function(data) {
                    return callback(ERROR_FAILED, data);
                },
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                dataType: 'json'
            });
        } catch (e) {
            return callback(ERROR_FAILED);
        }

        //sessionid = xyz
        //appid = 535690
        //assetid = 4830605461
        //contextid = 6
        //goo_value_expected = 10
        //http://steamcommunity.com/my/ajaxgrindintogoo/
    }


    // Unpacks the booster pack.
    SteamMarket.prototype.unpackBoosterPack = function(item, callback) {
        try {
            var sessionId = readCookie('sessionid');
            $.ajax({
                type: "POST",
                url: this.inventoryUrlBase + 'ajaxunpackbooster/',
                data: {
                    sessionid: sessionId,
                    appid: item.market_fee_app,
                    communityitemid: item.assetid
                },
                success: function(data) {
                    callback(ERROR_SUCCESS, data);
                },
                error: function(data) {
                    return callback(ERROR_FAILED, data);
                },
                crossDomain: true,
                xhrFields: {
                    withCredentials: true
                },
                dataType: 'json'
            });
        } catch (e) {
            return callback(ERROR_FAILED);
        }

        //sessionid = xyz
        //appid = 535690
        //communityitemid = 4830605461
        //http://steamcommunity.com/my/ajaxunpackbooster/
    }

    // Get the current price history for an item.
    SteamMarket.prototype.getCurrentPriceHistory = function(appid, market_name, callback) {
        var url = window.location.protocol +
            '//steamcommunity.com/market/pricehistory/?appid=' +
            appid +
            '&market_hash_name=' +
            market_name;

        $.get(url,
                function(data) {
                    if (!data || !data.success || !data.prices) {
                        callback(ERROR_DATA);
                        return;
                    }

                    // Multiply prices so they're in pennies.
                    for (var i = 0; i < data.prices.length; i++) {
                        data.prices[i][1] *= 100;
                        data.prices[i][2] = parseInt(data.prices[i][2]);
                    }

                    // Store the price history in the session storage.
                    var storage_hash = 'pricehistory_' + appid + '+' + market_name;
                    storageSession.setItem(storage_hash, data.prices);

                    callback(ERROR_SUCCESS, data.prices, false);
                },
                'json')
            .fail(function(data) {
                if (!data || !data.responseJSON) {
                    return callback(ERROR_FAILED);
                }
                if (!data.responseJSON.success) {
                    callback(ERROR_DATA);
                    return;
                }
                return callback(ERROR_FAILED);
            });
    }

    // Get the item name id from a market item.
    //
    // This id never changes so we can store this in the persistent storage.
    SteamMarket.prototype.getMarketItemNameId = function(item, callback) {
        try {
            var market_name = getMarketHashName(item);
            if (market_name == null) {
                callback(ERROR_FAILED);
                return;
            }

            var appid = item.appid;
            var storage_hash = 'itemnameid_' + appid + '+' + market_name;

            storagePersistent.getItem(storage_hash)
                .then(function(value) {
                    if (value != null)
                        callback(ERROR_SUCCESS, value);
                    else
                        return market.getCurrentMarketItemNameId(appid, market_name, callback);
                })
                .catch(function(error) {
                    return market.getCurrentMarketItemNameId(appid, market_name, callback);
                });
        } catch (e) {
            return callback(ERROR_FAILED);
        }
    }

    // Get the item name id from a market item.
    SteamMarket.prototype.getCurrentMarketItemNameId = function(appid, market_name, callback) {
        var url = window.location.protocol + '//steamcommunity.com/market/listings/' + appid + '/' + market_name;
        $.get(url,
                function(page) {
                    var matches = /Market_LoadOrderSpread\( (.+) \);/.exec(page);
                    if (matches == null) {
                        callback(ERROR_DATA);
                        return;
                    }

                    var item_nameid = matches[1];

                    // Store the item name id in the persistent storage.
                    var storage_hash = 'itemnameid_' + appid + '+' + market_name;
                    storagePersistent.setItem(storage_hash, item_nameid);

                    callback(ERROR_SUCCESS, item_nameid);
                })
            .fail(function(e) {
                return callback(ERROR_FAILED, e.status);
            });
    };

    // Get the sales listings for this item in the market, with more information.
    //
    //{
    //"success" : 1,
    //"sell_order_table" : "<table class=\"market_commodity_orders_table\"><tr><th align=\"right\">Price<\/th><th align=\"right\">Quantity<\/th><\/tr><tr><td align=\"right\" class=\"\">0,04\u20ac<\/td><td align=\"right\">311<\/td><\/tr><tr><td align=\"right\" class=\"\">0,05\u20ac<\/td><td align=\"right\">895<\/td><\/tr><tr><td align=\"right\" class=\"\">0,06\u20ac<\/td><td align=\"right\">495<\/td><\/tr><tr><td align=\"right\" class=\"\">0,07\u20ac<\/td><td align=\"right\">174<\/td><\/tr><tr><td align=\"right\" class=\"\">0,08\u20ac<\/td><td align=\"right\">49<\/td><\/tr><tr><td align=\"right\" class=\"\">0,09\u20ac or more<\/td><td align=\"right\">41<\/td><\/tr><\/table>",
    //"sell_order_summary" : "<span class=\"market_commodity_orders_header_promote\">1965<\/span> for sale starting at <span class=\"market_commodity_orders_header_promote\">0,04\u20ac<\/span>",
    //"buy_order_table" : "<table class=\"market_commodity_orders_table\"><tr><th align=\"right\">Price<\/th><th align=\"right\">Quantity<\/th><\/tr><tr><td align=\"right\" class=\"\">0,03\u20ac<\/td><td align=\"right\">93<\/td><\/tr><\/table>",
    //"buy_order_summary" : "<span class=\"market_commodity_orders_header_promote\">93<\/span> requests to buy at <span class=\"market_commodity_orders_header_promote\">0,03\u20ac<\/span> or lower",
    //"highest_buy_order" : "3",
    //"lowest_sell_order" : "4",
    //"buy_order_graph" : [[0.03, 93, "93 buy orders at 0,03\u20ac or higher"]],
    //"sell_order_graph" : [[0.04, 311, "311 sell orders at 0,04\u20ac or lower"], [0.05, 1206, "1,206 sell orders at 0,05\u20ac or lower"], [0.06, 1701, "1,701 sell orders at 0,06\u20ac or lower"], [0.07, 1875, "1,875 sell orders at 0,07\u20ac or lower"], [0.08, 1924, "1,924 sell orders at 0,08\u20ac or lower"], [0.09, 1934, "1,934 sell orders at 0,09\u20ac or lower"], [0.1, 1936, "1,936 sell orders at 0,10\u20ac or lower"], [0.11, 1937, "1,937 sell orders at 0,11\u20ac or lower"], [0.12, 1944, "1,944 sell orders at 0,12\u20ac or lower"], [0.14, 1945, "1,945 sell orders at 0,14\u20ac or lower"]],
    //"graph_max_y" : 3000,
    //"graph_min_x" : 0.03,
    //"graph_max_x" : 0.14,
    //"price_prefix" : "",
    //"price_suffix" : "\u20ac"
    //}
    SteamMarket.prototype.getItemOrdersHistogram = function(item, cache, callback) {
        try {
            var market_name = getMarketHashName(item);
            if (market_name == null) {
                callback(ERROR_FAILED);
                return;
            }

            var appid = item.appid;

            if (cache) {
                var storage_hash = 'itemordershistogram_' + appid + '+' + market_name;
                storageSession.getItem(storage_hash)
                    .then(function(value) {
                        if (value != null)
                            callback(ERROR_SUCCESS, value, true);
                        else {
                            market.getCurrentItemOrdersHistogram(item, market_name, callback);
                        }
                    })
                    .catch(function(error) {
                        market.getCurrentItemOrdersHistogram(item, market_name, callback);
                    });
            } else {
                market.getCurrentItemOrdersHistogram(item, market_name, callback);
            }

        } catch (e) {
            return callback(ERROR_FAILED);
        }
    };

    // Get the sales listings for this item in the market, with more information.
    SteamMarket.prototype.getCurrentItemOrdersHistogram = function(item, market_name, callback) {
        market.getMarketItemNameId(item,
            function(error, item_nameid) {
                if (error) {
                    if (item_nameid != 429) // 429 = Too many requests made.
                        callback(ERROR_DATA);
                    else
                        callback(ERROR_FAILED);
                    return;
                }
                var url = window.location.protocol +
                    '//steamcommunity.com/market/itemordershistogram?language=schinese&currency=' +
                    currencyId +
                    '&item_nameid=' +
                    item_nameid +
                    '&two_factor=0';

                $.get(url,
                        function(histogram) {
                            // Store the histogram in the session storage.
                            var storage_hash = 'itemordershistogram_' + item.appid + '+' + market_name;
                            storageSession.setItem(storage_hash, histogram);

                            callback(ERROR_SUCCESS, histogram, false);
                        })
                    .fail(function() {
                        return callback(ERROR_FAILED, null);
                    });
            });
    };

    // Calculate the price before fees (seller price) from the buyer price
    SteamMarket.prototype.getPriceBeforeFees = function(price, item) {
        var publisherFee = -1;

        if (item != null) {
            if (item.market_fee != null)
                publisherFee = item.market_fee;
            else if (item.description != null && item.description.market_fee != null)
                publisherFee = item.description.market_fee;
        }

        if (publisherFee == -1) {
            if (this.walletInfo != null)
                publisherFee = this.walletInfo['wallet_publisher_fee_percent_default'];
            else
                publisherFee = 0.10;
        }

        price = Math.round(price);
        var feeInfo = CalculateFeeAmount(price, publisherFee, this.walletInfo);
        return price - feeInfo.fees;
    };

    // Calculate the buyer price from the seller price
    SteamMarket.prototype.getPriceIncludingFees = function(price, item) {
        var publisherFee = -1;
        if (item != null) {
            if (item.market_fee != null)
                publisherFee = item.market_fee;
            else if (item.description != null && item.description.market_fee != null)
                publisherFee = item.description.market_fee;
        }
        if (publisherFee == -1) {
            if (this.walletInfo != null)
                publisherFee = this.walletInfo['wallet_publisher_fee_percent_default'];
            else
                publisherFee = 0.10;
        }

        price = Math.round(price);
        var feeInfo = CalculateAmountToSendForDesiredReceivedAmount(price, publisherFee, this.walletInfo);
        return feeInfo.amount;
    };
    //#endregion

    function replaceAll(str, find, replace) {
        return str.replace(new RegExp(find, 'g'), replace);
    }

    // Cannot use encodeURI / encodeURIComponent, Steam only escapes certain characters.
    function escapeURI(name) {
        var previousName = '';
        while (previousName != name) {
            previousName = name;
            name = name.replace('?', '%3F')
                .replace('#', '%23')
                .replace('	', '%09');
        }
        return name;
    }

    //#region Steam Market / Inventory helpers
    function getMarketHashName(item) {
        if (item == null)
            return null;

        if (item.description != null && item.description.market_hash_name != null)
            return escapeURI(item.description.market_hash_name);

        if (item.description != null && item.description.name != null)
            return escapeURI(item.description.name);

        if (item.market_hash_name != null)
            return escapeURI(item.market_hash_name);

        if (item.name != null)
            return escapeURI(item.name);

        return null;
    }

    function getIsCrate(item) {
        if (item == null)
            return false;
        // This is available on the inventory page.
        var tags = item.tags != null ?
            item.tags :
            (item.description != null && item.description.tags != null ?
                item.description.tags :
                null);
        if (tags != null) {
            var isTaggedAsCrate = false;
            tags.forEach(function (arrayItem) {
                if (arrayItem.category == 'Type')
                    if (arrayItem.internal_name == 'Supply Crate')
                        isTaggedAsCrate = true;
            });
            if (isTaggedAsCrate)
                return true;
        }
    }

    function getIsTradingCard(item) {
        if (item == null)
            return false;

        // This is available on the inventory page.
        var tags = item.tags != null ?
            item.tags :
            (item.description != null && item.description.tags != null ?
                item.description.tags :
                null);
        if (tags != null) {
            var isTaggedAsTradingCard = false;
            tags.forEach(function(arrayItem) {
                if (arrayItem.category == 'item_class')
                    if (arrayItem.internal_name == 'item_class_2') // trading card.
                        isTaggedAsTradingCard = true;
            });
            if (isTaggedAsTradingCard)
                return true;
        }

        // This is available on the market page.
        if (item.owner_actions != null) {
            for (var i = 0; i < item.owner_actions.length; i++) {
                if (item.owner_actions[i].link == null)
                    continue;

                // Cards include a link to the gamecard page.
                // For example: "http://steamcommunity.com/my/gamecards/503820/".
                if (item.owner_actions[i].link.toString().toLowerCase().includes('gamecards'))
                    return true;
            }
        }

        // A fallback for the market page (only works with language on English).
        if (item.type != null && item.type.toLowerCase().includes('trading card'))
            return true;

        return false;
    }

    function getIsFoilTradingCard(item) {
        if (!getIsTradingCard(item))
            return false;

        // This is available on the inventory page.
        var tags = item.tags != null ?
            item.tags :
            (item.description != null && item.description.tags != null ?
                item.description.tags :
                null);
        if (tags != null) {
            var isTaggedAsFoilTradingCard = false;
            tags.forEach(function(arrayItem) {
                if (arrayItem.category == 'cardborder')
                    if (arrayItem.internal_name == 'cardborder_1') // foil border.
                        isTaggedAsFoilTradingCard = true;
            });
            if (isTaggedAsFoilTradingCard)
                return true;
        }

        // This is available on the market page.
        if (item.owner_actions != null) {
            for (var i = 0; i < item.owner_actions.length; i++) {
                if (item.owner_actions[i].link == null)
                    continue;

                // Cards include a link to the gamecard page.
                // The border parameter specifies the foil cards.
                // For example: "http://steamcommunity.com/my/gamecards/503820/?border=1".
                if (item.owner_actions[i].link.toString().toLowerCase().includes('gamecards') &&
                    item.owner_actions[i].link.toString().toLowerCase().includes('border'))
                    return true;
            }
        }

        // A fallback for the market page (only works with language on English).
        if (item.type != null && item.type.toLowerCase().includes('foil trading card'))
            return true;

        return false;
    }

    function CalculateFeeAmount(amount, publisherFee, walletInfo) {
        if (walletInfo == null || !walletInfo['wallet_fee']) {
            return {
                fees: 0
            };
        }

        publisherFee = (publisherFee == null) ? 0 : publisherFee;
        // Since CalculateFeeAmount has a Math.floor, we could be off a cent or two. Let's check:
        var iterations = 0; // shouldn't be needed, but included to be sure nothing unforseen causes us to get stuck
        var nEstimatedAmountOfWalletFundsReceivedByOtherParty =
            parseInt((amount - parseInt(walletInfo['wallet_fee_base'])) /
                (parseFloat(walletInfo['wallet_fee_percent']) + parseFloat(publisherFee) + 1));
        var bEverUndershot = false;
        var fees = CalculateAmountToSendForDesiredReceivedAmount(nEstimatedAmountOfWalletFundsReceivedByOtherParty,
            publisherFee,
            walletInfo);
        while (fees.amount != amount && iterations < 10) {
            if (fees.amount > amount) {
                if (bEverUndershot) {
                    fees = CalculateAmountToSendForDesiredReceivedAmount(
                        nEstimatedAmountOfWalletFundsReceivedByOtherParty - 1,
                        publisherFee,
                        walletInfo);
                    fees.steam_fee += (amount - fees.amount);
                    fees.fees += (amount - fees.amount);
                    fees.amount = amount;
                    break;
                } else {
                    nEstimatedAmountOfWalletFundsReceivedByOtherParty--;
                }
            } else {
                bEverUndershot = true;
                nEstimatedAmountOfWalletFundsReceivedByOtherParty++;
            }
            fees = CalculateAmountToSendForDesiredReceivedAmount(nEstimatedAmountOfWalletFundsReceivedByOtherParty,
                publisherFee,
                walletInfo);
            iterations++;
        }
        // fees.amount should equal the passed in amount
        return fees;
    }

    // Clamps cur between min and max (inclusive).
    function clamp(cur, min, max) {
        if (cur < min)
            cur = min;

        if (cur > max)
            cur = max;

        return cur;
    }

    // Strangely named function, it actually works out the fees and buyer price for a seller price
    function CalculateAmountToSendForDesiredReceivedAmount(receivedAmount, publisherFee, walletInfo) {
        if (walletInfo == null || !walletInfo['wallet_fee']) {
            return {
                amount: receivedAmount
            };
        }

        publisherFee = (publisherFee == null) ? 0 : publisherFee;
        var nSteamFee = parseInt(Math.floor(Math.max(receivedAmount * parseFloat(walletInfo['wallet_fee_percent']),
                walletInfo['wallet_fee_minimum']) +
            parseInt(walletInfo['wallet_fee_base'])));
        var nPublisherFee = parseInt(Math.floor(publisherFee > 0 ? Math.max(receivedAmount * publisherFee, 1) : 0));
        var nAmountToSend = receivedAmount + nSteamFee + nPublisherFee;
        return {
            steam_fee: nSteamFee,
            publisher_fee: nPublisherFee,
            fees: nSteamFee + nPublisherFee,
            amount: parseInt(nAmountToSend)
        };
    }

    function readCookie(name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ')
                c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0)
                return decodeURIComponent(c.substring(nameEQ.length, c.length));
        }
        return null;
    }

    function isRetryMessage(message) {
        var messageList = [
            "在上一个操作完成之前，您不能出售任何物品。",
            "列出您的物品时出现问题。刷新页面并重试。",
            "我们无法连接到游戏物品服务器。游戏物品服务器可能已经关闭，或 Steam 可能正面临临时连接问题。您的列表尚未创建。请刷新页面并重试。"
        ];

        return messageList.indexOf(message) !== -1;
    }
    //#endregion

    //#region Logging
    var userScrolled = false;
    var logger = document.createElement('div');
    logger.setAttribute('id', 'logger');

    function updateScroll() {
        if (!userScrolled) {
            var element = document.getElementById("logger");
            element.scrollTop = element.scrollHeight;
        }
    }

    function logDOM(text) {
        logger.innerHTML += text + '<br/>';

        updateScroll();
    }

    function clearLogDOM() {
        logger.innerHTML = '';

        updateScroll();
    }

    function logConsole(text) {
        if (enableConsoleLog) {
            console.log(text);
        }
    }
    //#endregion

    //#region Inventory
    if (currentPage == PAGE_INVENTORY) {

        function onQueueDrain() {
            if (itemQueue.length() == 0 && sellQueue.length() == 0 && scrapQueue.length() == 0 && boosterQueue.length() == 0) {
                $('#inventory_items_spinner').remove();
            }
        }

        function updateTotals() {
            if ($('#loggerTotal').length == 0) {
                $(logger).parent().append('<div id="loggerTotal"></div>');
            }

            var totals = document.getElementById('loggerTotal');
            totals.innerHTML = '';

            if (totalPriceWithFeesOnMarket > 0) {
                totals.innerHTML += '<div><strong>累计上架物品总价为 ' +
                    (totalPriceWithFeesOnMarket / 100.0).toFixed(2) +
                    currencySymbol +
                    ', 你将会获得 ' +
                    (totalPriceWithoutFeesOnMarket / 100).toFixed(2) +
                    currencySymbol +
                    '.</strong></div>';
            }
            if (totalScrap > 0) {
                totals.innerHTML += '<div><strong>总共分解：' + totalScrap + '</strong></div>';
            }
        }

        var sellQueue = async.queue(function(task, next) {
                market.sellItem(task.item,
                    task.sellPrice,
                    function(err, data) {
                        totalNumberOfProcessedQueueItems++;

                        var digits = getNumberOfDigits(totalNumberOfQueuedItems);
                        var itemId = task.item.assetid || task.item.id;
                        var itemName = task.item.name || task.item.description.name;
                        var padLeft = padLeftZero('' + totalNumberOfProcessedQueueItems, digits) + ' / ' + totalNumberOfQueuedItems;

                        if (!err) {
                            logDOM(padLeft +
                                ' - ' +
                                itemName +
                                ' 已添加至市场，售价为 ' +
                                (market.getPriceIncludingFees(task.sellPrice) / 100.0).toFixed(2) +
                                currencySymbol +
                                ', 你将收到 ' +
                                (task.sellPrice / 100.0).toFixed(2) + currencySymbol +
                                '.');

                            $('#' + task.item.appid + '_' + task.item.contextid + '_' + itemId)
                                .css('background', COLOR_SUCCESS);

                            totalPriceWithoutFeesOnMarket += task.sellPrice;
                            totalPriceWithFeesOnMarket += market.getPriceIncludingFees(task.sellPrice);
                            updateTotals();
                        } else if (data != null && isRetryMessage(data.message)) {
                            logDOM(padLeft +
                                ' - ' +
                                itemName +
                                ' 正在重试列出物品，原因为 ' +
                                data.message[0].toLowerCase() +
                                data.message.slice(1));

                            totalNumberOfProcessedQueueItems--;
                            sellQueue.unshift(task);
                            sellQueue.pause();

                            setTimeout(function() {
                                sellQueue.resume();
                            }, getRandomInt(30000, 45000));
                        } else {
                            if (data != null && data.responseJSON != null && data.responseJSON.message != null) {
                                logDOM(padLeft +
                                    ' - ' +
                                    itemName +
                                    ' 上架市场失败，原因为 ' +
                                    data.responseJSON.message[0].toLowerCase() +
                                    data.responseJSON.message.slice(1));
                            } else
                                logDOM(padLeft + ' - ' + itemName + ' 上架市场失败');

                            $('#' + task.item.appid + '_' + task.item.contextid + '_' + itemId)
                                .css('background', COLOR_ERROR);
                        }

                        next();
                    });
            },
            1);

        sellQueue.drain = function() {
            onQueueDrain();
        }

        function sellAllItems(appId) {
            loadAllInventories().then(function() {
                    var items = getInventoryItems();
                    var filteredItems = [];

                    items.forEach(function(item) {
                        if (!item.marketable) {
                            return;
                        }

                        filteredItems.push(item);
                    });

                    sellItems(filteredItems);
                },
                function() {
                    logDOM('无法检索库存...');
                });
        }

        function sellAllCards() {
            loadAllInventories().then(function() {
                    var items = getInventoryItems();
                    var filteredItems = [];

                    items.forEach(function(item) {
                        if (!getIsTradingCard(item) || !item.marketable) {
                            return;
                        }

                        filteredItems.push(item);
                    });

                    sellItems(filteredItems);
                },
                function() {
                    logDOM('无法检索库存...');
                });
        }

        function sellAllCrates() {
            loadAllInventories().then(function () {
                    var items = getInventoryItems();
                    var filteredItems = [];
                    items.forEach(function (item) {
                        if (!getIsCrate(item) || !item.marketable) {
                            return;
                        }
                        filteredItems.push(item);
                    });

                    sellItems(filteredItems);
                },
                function() {
                    logDOM('无法检索库存...');
                });
        }

        var scrapQueue = async.queue(function(item, next) {
            scrapQueueWorker(item, function(success) {
                if (success) {
                    setTimeout(function() {
                        next();
                    }, 250);
                } else {
                    var delay = numberOfFailedRequests > 1 ?
                        getRandomInt(30000, 45000) :
                        getRandomInt(1000, 1500);

                    if (numberOfFailedRequests > 3)
                        numberOfFailedRequests = 0;

                    setTimeout(function() {
                        next();
                    }, delay);
                }
            });
        }, 1);

        scrapQueue.drain = function() {
            onQueueDrain();
        }

        function scrapQueueWorker(item, callback) {
            var failed = 0;
            var itemName = item.name || item.description.name;
            var itemId = item.assetid || item.id;

            market.getGooValue(item,
                function(err, goo) {
                    totalNumberOfProcessedQueueItems++;

                    var digits = getNumberOfDigits(totalNumberOfQueuedItems);
                    var padLeft = padLeftZero('' + totalNumberOfProcessedQueueItems, digits) + ' / ' + totalNumberOfQueuedItems;

                    if (err != ERROR_SUCCESS) {
                        logConsole('无法获取 ' + itemName + ' 分解后的宝石数。');
                        logDOM(padLeft + ' - ' + itemName + ' 由于缺少宝石数而未变成宝石');

                        $('#' + item.appid + '_' + item.contextid + '_' + itemId).css('background', COLOR_ERROR);
                        return callback(false);
                    }

                    item.goo_value_expected = parseInt(goo.goo_value);

                    market.grindIntoGoo(item,
                        function(err, result) {
                            if (err != ERROR_SUCCESS) {
                                logConsole('无法将' + itemName + '分解为宝石。');
                                logDOM(padLeft + ' - ' + itemName + ' 由于未知错误，未分解为宝石');

                                $('#' + item.appid + '_' + item.contextid + '_' + itemId).css('background', COLOR_ERROR);
                                return callback(false);
                            }

                            logConsole('============================')
                            logConsole(itemName);
                            logConsole('分解为 ' + goo.goo_value + '个 宝石');
                            logDOM(padLeft + ' - ' + itemName + ' 已分解为 ' + item.goo_value_expected + '个 宝石.');
                            $('#' + item.appid + '_' + item.contextid + '_' + itemId).css('background', COLOR_SUCCESS);

                            totalScrap += item.goo_value_expected;
                            updateTotals();

                            callback(true);
                        });
                });
        }

        var boosterQueue = async.queue(function(item, next) {
            boosterQueueWorker(item, function(success) {
                if (success) {
                    setTimeout(function() {
                        next();
                    }, 250);
                } else {
                    var delay = numberOfFailedRequests > 1 ?
                        getRandomInt(30000, 45000) :
                        getRandomInt(1000, 1500);

                    if (numberOfFailedRequests > 3)
                        numberOfFailedRequests = 0;

                    setTimeout(function() {
                        next();
                    }, delay);
                }
            });
        }, 1);

        boosterQueue.drain = function() {
            onQueueDrain();
        }

        function boosterQueueWorker(item, callback) {
            var failed = 0;
            var itemName = item.name || item.description.name;
            var itemId = item.assetid || item.id;

            market.unpackBoosterPack(item,
                function(err, goo) {
                    totalNumberOfProcessedQueueItems++;

                    var digits = getNumberOfDigits(totalNumberOfQueuedItems);
                    var padLeft = padLeftZero('' + totalNumberOfProcessedQueueItems, digits) + ' / ' + totalNumberOfQueuedItems;

                    if (err != ERROR_SUCCESS) {
                        logConsole('无法拆开补充包 ' + itemName);
                        logDOM(padLeft + ' - ' + itemName + ' 拆包失败.');

                        $('#' + item.appid + '_' + item.contextid + '_' + itemId).css('background', COLOR_ERROR);
                        return callback(false);
                    }

                    logDOM(padLeft + ' - ' + itemName + ' 拆包成功.');
                    $('#' + item.appid + '_' + item.contextid + '_' + itemId).css('background', COLOR_SUCCESS);

                    callback(true);
                });
        }


        // Turns the selected items into gems.
        function turnSelectedItemsIntoGems() {
            var ids = getSelectedItems();

            loadAllInventories().then(function() {
                var items = getInventoryItems();

                var numberOfQueuedItems = 0;
                items.forEach(function(item) {
                    // Ignored queued items.
                    if (item.queued != null) {
                        return;
                    }

                    if (item.owner_actions == null) {
                        return;
                    }

                    var canTurnIntoGems = false;
                    for (var owner_action in item.owner_actions) {
                        if (item.owner_actions[owner_action].link != null && item.owner_actions[owner_action].link.includes('GetGooValue')) {
                            canTurnIntoGems = true;
                        }
                    }

                    if (!canTurnIntoGems)
                        return;

                    var itemId = item.assetid || item.id;
                    if (ids.indexOf(itemId) !== -1) {
                        item.queued = true;
                        scrapQueue.push(item);
                        numberOfQueuedItems++;
                    }
                });

                if (numberOfQueuedItems > 0) {
                    totalNumberOfQueuedItems += numberOfQueuedItems;

                    $('#inventory_items_spinner').remove();
                    $('#inventory_sell_buttons').append('<div id="inventory_items_spinner">' +
                        spinnerBlock +
                        '<div style="text-align:center">正在处理 ' + numberOfQueuedItems + '个 物品</div>' +
                        '</div>');
                }
            }, function() {
                logDOM('无法检索库存...');
            });
        }

        // Unpacks the selected booster packs.
        function unpackSelectedBoosterPacks() {
            var ids = getSelectedItems();

            loadAllInventories().then(function() {
                var items = getInventoryItems();

                var numberOfQueuedItems = 0;
                items.forEach(function(item) {
                    // Ignored queued items.
                    if (item.queued != null) {
                        return;
                    }

                    if (item.owner_actions == null) {
                        return;
                    }

                    var canOpenBooster = false;
                    for (var owner_action in item.owner_actions) {
                        if (item.owner_actions[owner_action].link != null && item.owner_actions[owner_action].link.includes('OpenBooster')) {
                            canOpenBooster = true;
                        }
                    }

                    if (!canOpenBooster)
                        return;

                    var itemId = item.assetid || item.id;
                    if (ids.indexOf(itemId) !== -1) {
                        item.queued = true;
                        boosterQueue.push(item);
                        numberOfQueuedItems++;
                    }
                });

                if (numberOfQueuedItems > 0) {
                    totalNumberOfQueuedItems += numberOfQueuedItems;

                    $('#inventory_items_spinner').remove();
                    $('#inventory_sell_buttons').append('<div id="inventory_items_spinner">' +
                        spinnerBlock +
                        '<div style="text-align:center">正在处理 ' + numberOfQueuedItems + '个 物品</div>' +
                        '</div>');
                }
            }, function() {
                logDOM('无法检索库存...');
            });
        }

        function sellSelectedItems() {
            getInventorySelectedMarketableItems(function(items) {
                sellItems(items);
            });
        }

        function canSellSelectedItemsManually(items) {
            // We have to construct an URL like this
            // https://steamcommunity.com/market/multisell?appid=730&contextid=2&items[]=Falchion%20Case&qty[]=100
            var appid = items[0].appid;
            var contextid = items[0].contextid;

            var hasInvalidItem = false;

            items.forEach(function(item) {
				if (item.contextid != contextid || item.commodity == false)
				    hasInvalidItem = true;
            });

            return !hasInvalidItem;
        }

        function sellSelectedItemsManually() {
            getInventorySelectedMarketableItems(function(items) {
                // We have to construct an URL like this
                // https://steamcommunity.com/market/multisell?appid=730&contextid=2&items[]=Falchion%20Case&qty[]=100

				var appid = items[0].appid;
                var contextid = items[0].contextid;

                var itemsWithQty = {};

                items.forEach(function(item) {
                   itemsWithQty[item.market_hash_name] = itemsWithQty[item.market_hash_name] + 1 || 1;
                });

                var itemsString = '';
                for (var itemName in itemsWithQty) {
                    itemsString += '&items[]=' + encodeURI(itemName) + '&qty[]=' + itemsWithQty[itemName];
                }

                var baseUrl = 'https://steamcommunity.com/market/multisell';
                var redirectUrl = baseUrl + '?appid=' + appid + '&contextid=' + contextid + itemsString;

                var dialog = unsafeWindow.ShowDialog('Steam Economy Enhancer', '<iframe frameBorder="0" height="650" width="900" src="' + redirectUrl + '"></iframe>');
                dialog.OnDismiss(function() {
                    items.forEach(function(item) {
                        var itemId = item.assetid || item.id;
                        $('#' + item.appid + '_' + item.contextid + '_' + itemId).css('background', COLOR_PENDING);
                    });
                });
            });
        }

        function sellItems(items) {
            if (items.length == 0) {
                logDOM('这些物品无法被上架至市场...');

                return;
            }

            var numberOfQueuedItems = 0;

            items.forEach(function(item, index, array) {
                // Ignored queued items.
                if (item.queued != null) {
                    return;
                }

                item.queued = true;
                var itemId = item.assetid || item.id;
                item.ignoreErrors = false;
                itemQueue.push(item);
                numberOfQueuedItems++;
            });

            if (numberOfQueuedItems > 0) {
                totalNumberOfQueuedItems += numberOfQueuedItems;

                $('#inventory_items_spinner').remove();
                $('#inventory_sell_buttons').append('<div id="inventory_items_spinner">' +
                    spinnerBlock +
                    '<div style="text-align:center">正在处理 ' + numberOfQueuedItems + '个 物品</div>' +
                    '</div>');
            }
        }

        var itemQueue = async.queue(function(item, next) {
            itemQueueWorker(item,
                item.ignoreErrors,
                function(success, cached) {
                    if (success) {
                        setTimeout(function() {
                                next();
                            },
                            cached ? 0 : getRandomInt(1000, 1500));
                    } else {
                        if (!item.ignoreErrors) {
                            item.ignoreErrors = true;
                            itemQueue.push(item);
                        }

                        var delay = numberOfFailedRequests > 1 ?
                            getRandomInt(30000, 45000) :
                            getRandomInt(1000, 1500);

                        if (numberOfFailedRequests > 3)
                            numberOfFailedRequests = 0;

                        setTimeout(function() {
                                next();
                            },
                            cached ? 0 : delay);
                    }
                });
        }, 1);

        function itemQueueWorker(item, ignoreErrors, callback) {
            var priceInfo = getPriceInformationFromItem(item);

            var failed = 0;
            var itemName = item.name || item.description.name;

            market.getPriceHistory(item,
                true,
                function(err, history, cachedHistory) {
                    if (err) {
                        logConsole('无法获取 ' + itemName + ' 的价格历史');

                        if (err == ERROR_FAILED)
                            failed += 1;
                    }

                    market.getItemOrdersHistogram(item,
                        true,
                        function(err, histogram, cachedListings) {
                            if (err) {
                                logConsole('无法获取 ' + itemName + ' 的订单直方图');

                                if (err == ERROR_FAILED)
                                    failed += 1;
                            }

                            if (failed > 0 && !ignoreErrors) {
                                return callback(false, cachedHistory && cachedListings);
                            }

                            logConsole('============================')
                            logConsole(itemName);

                            var sellPrice = calculateSellPriceBeforeFees(history,
                                histogram,
                                true,
                                priceInfo.minPriceBeforeFees,
                                priceInfo.maxPriceBeforeFees);


                            logConsole('售价：' +
                                sellPrice / 100.0 +
                                ' (' +
                                market.getPriceIncludingFees(sellPrice) / 100.0 +
                                ')');

                            sellQueue.push({
                                item: item,
                                sellPrice: sellPrice
                            });

                            return callback(true, cachedHistory && cachedListings);
                        });
                });
        }

        // Initialize the inventory UI.
        function initializeInventoryUI() {
            var isOwnInventory = unsafeWindow.g_ActiveUser.strSteamId == unsafeWindow.g_steamID;
            var previousSelection = -1; // To store the index of the previous selection.
            updateInventoryUI(isOwnInventory);

            $('.games_list_tabs').on('click',
                '*',
                function() {
                    updateInventoryUI(isOwnInventory);
                });

            // Ignore selection on other user's inventories.
            if (!isOwnInventory)
                return;

            // Steam adds 'display:none' to items while searching. These should not be selected while using shift/ctrl.
            var filter = ".itemHolder:not([style*=none])";
            $('#inventories').selectable({
                filter: filter,
                selecting: function(e, ui) {
                    // Get selected item index.
                    var selectedIndex = $(ui.selecting.tagName, e.target).index(ui.selecting);

                    // If shift key was pressed and there is previous - select them all.
                    if (e.shiftKey && previousSelection > -1) {
                        $(ui.selecting.tagName, e.target)
                            .slice(Math.min(previousSelection, selectedIndex),
                                1 + Math.max(previousSelection, selectedIndex)).each(function() {
                                if ($(this).is(filter)) {
                                    $(this).addClass('ui-selected');
                                }
                            });
                        previousSelection = -1; // Reset previous.
                    } else {
                        previousSelection = selectedIndex; // Save previous.
                    }
                },
                selected: function(e, ui) {
                    updateInventorySelection(ui.selected);
                }
            });
        }

        // Gets the selected items in the inventory.
        function getSelectedItems() {
            var ids = [];
            $('.inventory_ctn').each(function() {
                $(this).find('.inventory_page').each(function() {
                    var inventory_page = this;

                    $(inventory_page).find('.itemHolder').each(function() {
                        if (!$(this).hasClass('ui-selected'))
                            return;

                        $(this).find('.item').each(function() {
                            var matches = this.id.match(/_(\-?\d+)$/);
                            if (matches) {
                                ids.push(matches[1]);
                            }
                        });
                    });
                });
            });

            return ids;
        }

        // Gets the selected and marketable items in the inventory.
        function getInventorySelectedMarketableItems(callback) {
            var ids = getSelectedItems();

            loadAllInventories().then(function() {
                var items = getInventoryItems();
                var filteredItems = [];

                items.forEach(function(item) {
                    if (!item.marketable) {
                        return;
                    }

                    var itemId = item.assetid || item.id;
                    if (ids.indexOf(itemId) !== -1) {
                        filteredItems.push(item);
                    }
                });

                callback(filteredItems);
            }, function() {
                logDOM('无法检索库存...');
            });
        }

        // Gets the selected and gemmable items in the inventory.
        function getInventorySelectedGemsItems(callback) {
            var ids = getSelectedItems();

            loadAllInventories().then(function() {
                var items = getInventoryItems();
                var filteredItems = [];

                items.forEach(function(item) {
                    var canTurnIntoGems = false;
                    for (var owner_action in item.owner_actions) {
                        if (item.owner_actions[owner_action].link != null && item.owner_actions[owner_action].link.includes('GetGooValue')) {
                            canTurnIntoGems = true;
                        }
                    }

                    if (!canTurnIntoGems)
                        return;

                    var itemId = item.assetid || item.id;
                    if (ids.indexOf(itemId) !== -1) {
                        filteredItems.push(item);
                    }
                });

                callback(filteredItems);
            }, function() {
                logDOM('无法检索库存...');
            });
        }

        // Gets the selected and booster pack items in the inventory.
        function getInventorySelectedBoosterPackItems(callback) {
            var ids = getSelectedItems();

            loadAllInventories().then(function() {
                var items = getInventoryItems();
                var filteredItems = [];

                items.forEach(function(item) {
                    var canOpenBooster = false;
                    for (var owner_action in item.owner_actions) {
                        if (item.owner_actions[owner_action].link != null && item.owner_actions[owner_action].link.includes('OpenBooster')) {
                            canOpenBooster = true;
                        }
                    }

                    if (!canOpenBooster)
                        return;

                    var itemId = item.assetid || item.id;
                    if (ids.indexOf(itemId) !== -1) {
                        filteredItems.push(item);
                    }
                });

                callback(filteredItems);
            }, function() {
                logDOM('无法检索库存...');
            });
        }

        // Updates the (selected) sell ... items button.
        function updateSellSelectedButton() {
            getInventorySelectedMarketableItems(function(items) {
                var selectedItems = items.length;
                if (items.length == 0) {
                    $('.sell_selected').hide();
                    $('.sell_manual').hide();
                } else {
                    $('.sell_selected').show();
                    if (canSellSelectedItemsManually(items)) {
                        $('.sell_manual').show();
                        $('.sell_manual > span').text('手动出售 ' + selectedItems + '个 物品');
                    } else {
                        $('.sell_manual').hide();
                    }
                    $('.sell_selected > span').text('出售 ' + selectedItems + '个 物品');
                }
            });
        }

        // Updates the (selected) turn into ... gems button.
        function updateTurnIntoGemsButton() {
            getInventorySelectedGemsItems(function(items) {
                var selectedItems = items.length;
                if (items.length == 0) {
                    $('.turn_into_gems').hide();
                } else {
                    $('.turn_into_gems').show();
                    $('.turn_into_gems > span')
                        .text('分解 ' + selectedItems + '个 物品为宝石');
                }
            });
        }

        // Updates the (selected) open ... booster packs button.
        function updateOpenBoosterPacksButton() {
            getInventorySelectedBoosterPackItems(function(items) {
                var selectedItems = items.length;
                if (items.length == 0) {
                    $('.unpack_booster_packs').hide();
                } else {
                    $('.unpack_booster_packs').show();
                    $('.unpack_booster_packs > span')
                        .text('拆开 ' + selectedItems + '个 补充包');
                }
            });
        }

        function updateInventorySelection(item) {
            updateSellSelectedButton();
            updateTurnIntoGemsButton();
            updateOpenBoosterPacksButton();

            // Wait until g_ActiveInventory.selectedItem is identical to the selected UI item.
            // This also makes sure that the new - and correct - item_info (iteminfo0 or iteminfo1) is visible.
            var selectedItemIdUI = $('div', item).attr('id');
            var selectedItemIdInventory = getActiveInventory().selectedItem.appid +
                '_' +
                getActiveInventory().selectedItem.contextid +
                '_' +
                getActiveInventory().selectedItem.assetid;
            if (selectedItemIdUI !== selectedItemIdInventory) {
                setTimeout(function() {
                    updateInventorySelection(item);
                }, 250);

                return;
            }

            var item_info = $('.inventory_iteminfo:visible').first();
            if (item_info.html().indexOf('checkout/sendgift/') > -1) // Gifts have no market information.
                return;

            // Use a 'hard' item id instead of relying on the selected item_info (sometimes Steam temporarily changes the correct item (?)).
            var item_info_id = item_info.attr('id');

            // Move scrap to bottom, this is of little interest.
            var scrap = $('#' + item_info_id + '_scrap_content');
            scrap.next().insertBefore(scrap);

            // Starting at prices are already retrieved in the table.
            //$('#' + item_info_id + '_item_market_actions > div:nth-child(1) > div:nth-child(2)')
            //    .remove(); // Starting at: x,xx.

            var market_hash_name = getMarketHashName(getActiveInventory().selectedItem);
            if (market_hash_name == null)
                return;

            var appid = getActiveInventory().selectedItem.appid;
            var item = {
                appid: parseInt(appid),
                description: {
                    market_hash_name: market_hash_name
                }
            };

            market.getItemOrdersHistogram(item,
                false,
                function(err, histogram) {
                    if (err) {
                        logConsole('无法获取 ' + (getActiveInventory().selectedItem.name || getActiveInventory().selectedItem.description.name) + ' 的订单直方图');
                        return;
                    }

                    var groupMain = $('<div id="listings_group">' +
                        '<div><div id="listings_sell">出售</div>' +
                        histogram.sell_order_table +
                        '</div>' +
                        '<div><div id="listings_buy">购买</div>' +
                        histogram.buy_order_table +
                        '</div>' +
                        '</div>');

                    $('#' + item_info_id + '_item_market_actions > div').after(groupMain);

                    var ownerActions = $('#' + item_info_id + '_item_owner_actions');
                    // ownerActions is hidden on other games' inventories, we need to show it to have a "Market" button visible
                    ownerActions.show();

                    ownerActions.append('<a class="btn_small btn_grey_white_innerfade" href="/market/listings/' + appid + '/' + market_hash_name + '"><span>在社区市场中查看</span></a>');
                    $('#' + item_info_id + '_item_market_actions > div:nth-child(1) > div:nth-child(1)').hide();

                    var isBoosterPack = getActiveInventory().selectedItem.name.toLowerCase().endsWith('booster pack');
                    if (isBoosterPack) {
                        var tradingCardsUrl = "/market/search?q=&category_753_Game%5B%5D=tag_app_" + getActiveInventory().selectedItem.market_fee_app + "&category_753_item_class%5B%5D=tag_item_class_2&appid=753";
                        ownerActions.append('<br/> <a class="btn_small btn_grey_white_innerfade" href="' + tradingCardsUrl + '"><span>在社区市场中查看可集换式卡牌</span></a>');
                    }


                    // Generate quick sell buttons.
                    var itemId = getActiveInventory().selectedItem.assetid || getActiveInventory().selectedItem.id;

                    // Ignored queued items.
                    if (getActiveInventory().selectedItem.queued != null) {
                        return;
                    }

                    var prices = [];

                    if (histogram != null && histogram.highest_buy_order != null) {
                        prices.push(parseInt(histogram.highest_buy_order));
                    }

                    if (histogram != null && histogram.lowest_sell_order != null) {
                        // Transaction volume must be separable into three or more parts (no matter if equal): valve+publisher+seller.
                        if (parseInt(histogram.lowest_sell_order) > 3) {
                            prices.push(parseInt(histogram.lowest_sell_order) - 1);
                        }
                        prices.push(parseInt(histogram.lowest_sell_order));
                    }

                    prices = prices.filter((v, i) => prices.indexOf(v) === i).sort((a, b) => a - b);

                    var buttons = '<div>';
                    var oributton = $('#' + item_info_id + '_item_market_actions > a.item_market_action_button', item_info);
                    buttons += '<a class="item_market_action_button item_market_action_button_green" style="margin-right: 4px; display: inline-block;" href="javascript:SellCurrentSelection()">'+oributton.html()+'</a>';
                    oributton.remove();
                    prices.forEach(function(e) {
                        buttons +=
                            '<a class="item_market_action_button item_market_action_button_green quick_sell" style="display: inline-block;" id="quick_sell' +
                            e +
                            '">' +
                            '<span class="item_market_action_button_edge item_market_action_button_left"></span>' +
                            '<span class="item_market_action_button_contents">' +
                            (e / 100.0) +
                            currencySymbol +
                            '</span>' +
                            '<span class="item_market_action_button_edge item_market_action_button_right"></span>' +
                            '<span class="item_market_action_button_preload"></span>' +
                            '</a>'
                    });
                    buttons += '</div>';

                    $('#' + item_info_id + '_item_market_actions', item_info).append(buttons);

                    $('#' + item_info_id + '_item_market_actions', item_info).append(
                        '<div style="display:flex">' +
                        '<input id="quick_sell_input" style="background-color: black;color: white;border: transparent;max-width:65px;text-align:center;" type="number" value="' + (histogram.lowest_sell_order / 100) + '" step="0.01" />' +
                        '&nbsp;<a class="item_market_action_button item_market_action_button_green quick_sell_custom">' +
                        '<span class="item_market_action_button_edge item_market_action_button_left"></span>' +
                        '<span class="item_market_action_button_contents">➜ 确认出售</span>' +
                        '<span class="item_market_action_button_edge item_market_action_button_right"></span>' +
                        '<span class="item_market_action_button_preload"></span>' +
                        '</a>' +
                        '</div>');

                    $('.quick_sell').on('click',
                        function() {
                            var price = $(this).attr('id').replace('quick_sell', '');
                            price = market.getPriceBeforeFees(price);

                            totalNumberOfQueuedItems++;

                            sellQueue.push({
                                item: getActiveInventory().selectedItem,
                                sellPrice: price
                            });
                        });

                    $('.quick_sell_custom').on('click',
                        function() {
                            var price = $('#quick_sell_input', $('#' + item_info_id + '_item_market_actions', item_info)).val() * 100;
                            price = market.getPriceBeforeFees(price);

                            totalNumberOfQueuedItems++;

                            sellQueue.push({
                                item: getActiveInventory().selectedItem,
                                sellPrice: price
                            });
                        });
                });
        }

        // Update the inventory UI.
        function updateInventoryUI(isOwnInventory) {
            // Remove previous containers (e.g., when a user changes inventory).
            $('#inventory_sell_buttons').remove();
            $('#price_options').remove();
            $('#inventory_reload_button').remove();

            $('#see_settings').remove();
            $('#global_action_menu')
                .prepend('<span id="see_settings"><a href="javascript:void(0)">⬖ SEE 设置 </a></span>');
            $('#see_settings').on('click', '*', () => openSettings());

            var appId = getActiveInventory().m_appid;
            var showMiscOptions = appId == 753;
            var TF2 = appId == 440;

            var sellButtons = $('<div id="inventory_sell_buttons" style="margin-bottom:12px;">' +
                '<a class="btn_green_white_innerfade btn_medium_wide sell_all separator-btn-right"><span>出售所有物品</span></a>' +
                '<a class="btn_green_white_innerfade btn_medium_wide sell_selected separator-btn-right" style="display:none"><span>出售所选物品</span></a>' +
                '<a class="btn_green_white_innerfade btn_medium_wide sell_manual separator-btn-right" style="display:none"><span>手动出售物品</span></a>' +
                (showMiscOptions ?
                    '<a class="btn_green_white_innerfade btn_medium_wide sell_all_cards separator-btn-right"><span>出售所有卡牌</span></a>' +
                    '<div style="margin-top:12px;">' +
                    '<a class="btn_darkblue_white_innerfade btn_medium_wide turn_into_gems separator-btn-right" style="display:none"><span>将选中物品分解为宝石</span></a>' +
                    '<a class="btn_darkblue_white_innerfade btn_medium_wide unpack_booster_packs separator-btn-right" style="display:none"><span>拆开选中的补充包</span></a>' +
                    '</div>' :
                    '') +
                (TF2 ? '<a class="btn_green_white_innerfade btn_medium_wide sell_all_crates separator-btn-right"><span>出售所有箱子</span></a>' : '') +
                '</div>');

            var reloadButton =
                $('<a id="inventory_reload_button" class="btn_darkblue_white_innerfade btn_medium_wide reload_inventory" style="margin-right:12px"><span>重新加载库存</span></a>');

            $('#inventory_logos')[0].style.height = 'auto';

            $('#inventory_applogo').hide(); // Hide the Steam/game logo, we don't need to see it twice.
            $('#inventory_applogo').after(logger);


            $("#logger").on('scroll',
                function() {
                    var hasUserScrolledToBottom =
                        $("#logger").prop('scrollHeight') - $("#logger").prop('clientHeight') <=
                        $("#logger").prop('scrollTop') + 1;
                    userScrolled = !hasUserScrolledToBottom;
                });

            // Only add buttons on the user's inventory.
            if (isOwnInventory) {
                $('#inventory_applogo').after(sellButtons);

                // Add bindings to sell buttons.
                $('.sell_all').on('click',
                    '*',
                    function() {
                        sellAllItems(appId);
                    });
                $('.sell_selected').on('click', '*', sellSelectedItems);
                $('.sell_manual').on('click', '*', sellSelectedItemsManually);
                $('.sell_all_cards').on('click', '*', sellAllCards);
                $('.sell_all_crates').on('click', '*', sellAllCrates);
                $('.turn_into_gems').on('click', '*', turnSelectedItemsIntoGems);
                $('.unpack_booster_packs').on('click', '*', unpackSelectedBoosterPacks);

            }

            $('.inventory_rightnav').prepend(reloadButton);
            $('.reload_inventory').on('click',
                '*',
                function() {
                    window.location.reload();
                });

            loadAllInventories().then(function() {
                    var updateInventoryPrices = function() {
                        if (getSettingWithDefault(SETTING_INVENTORY_PRICE_LABELS) == 1) {
                            setInventoryPrices(getInventoryItems());
                        }
                    };

                    // Load after the inventory is loaded.
                    updateInventoryPrices();

                    $('#inventory_pagecontrols').observe('childlist',
                        '*',
                        function(record) {
                            updateInventoryPrices();
                        });
                },
                function() {
                    logDOM('无法检索库存...');
                });
        }

        // Loads the specified inventories.
        function loadInventories(inventories) {
            return new Promise(function(resolve) {
                inventories.reduce(function(promise, inventory) {
                        return promise.then(function() {
                            return inventory.LoadCompleteInventory().done(function() {});
                        });
                    },
                    Promise.resolve());

                resolve();
            });
        }

        // Loads all inventories.
        function loadAllInventories() {
            var items = [];

            for (var child in getActiveInventory().m_rgChildInventories) {
                items.push(getActiveInventory().m_rgChildInventories[child]);
            }
            items.push(getActiveInventory());

            return loadInventories(items);
        }

        // Gets the inventory items from the active inventory.
        function getInventoryItems() {
            var arr = [];

            for (var child in getActiveInventory().m_rgChildInventories) {
                for (var key in getActiveInventory().m_rgChildInventories[child].m_rgAssets) {
                    var value = getActiveInventory().m_rgChildInventories[child].m_rgAssets[key];
                    if (typeof value === 'object') {
                        // Merges the description in the normal object, this is done to keep the layout consistent with the market page, which is also flattened.
                        Object.assign(value, value.description);
                        // Includes the id of the inventory item.
                        value['id'] = key;
                        arr.push(value);
                    }
                }
            }

            // Some inventories (e.g. BattleBlock Theater) do not have child inventories, they have just one.
            for (var key in getActiveInventory().m_rgAssets) {
                var value = getActiveInventory().m_rgAssets[key];
                if (typeof value === 'object') {
                    // Merges the description in the normal object, this is done to keep the layout consistent with the market page, which is also flattened.
                    Object.assign(value, value.description);
                    // Includes the id of the inventory item.
                    value['id'] = key;
                    arr.push(value);
                }
            }

            return arr;
        }
    }
    //#endregion

    //#region Inventory + Tradeoffer
    if (currentPage == PAGE_INVENTORY || currentPage == PAGE_TRADEOFFER) {

        // Gets the active inventory.
        function getActiveInventory() {
            return unsafeWindow.g_ActiveInventory;
        }

        // Sets the prices for the items.
        function setInventoryPrices(items) {
            inventoryPriceQueue.kill();

            items.forEach(function(item) {
                if (!item.marketable) {
                    return;
                }

                if (!$(item.element).is(":visible")) {
                    return;
                }

                inventoryPriceQueue.push(item);
            });
        }

        var inventoryPriceQueue = async.queue(function(item, next) {
                inventoryPriceQueueWorker(item,
                    false,
                    function(success, cached) {
                        if (success) {
                            setTimeout(function() {
                                    next();
                                },
                                cached ? 0 : getRandomInt(1000, 1500));
                        } else {
                            if (!item.ignoreErrors) {
                                item.ignoreErrors = true;
                                inventoryPriceQueue.push(item);
                            }

                            numberOfFailedRequests++;

                            var delay = numberOfFailedRequests > 1 ?
                                getRandomInt(30000, 45000) :
                                getRandomInt(1000, 1500);

                            if (numberOfFailedRequests > 3)
                                numberOfFailedRequests = 0;

                            setTimeout(function() {
                                next();
                            }, cached ? 0 : delay);
                        }
                    });
            },
            1);

        function inventoryPriceQueueWorker(item, ignoreErrors, callback) {
            var priceInfo = getPriceInformationFromItem(item);

            var failed = 0;
            var itemName = item.name || item.description.name;

            // Only get the market orders here, the history is not important to visualize the current prices.
            market.getItemOrdersHistogram(item,
                true,
                function(err, histogram, cachedListings) {
                    if (err) {
                        logConsole('无法获取 ' + itemName + ' 的订单历史直方图');

                        if (err == ERROR_FAILED)
                            failed += 1;
                    }

                    if (failed > 0 && !ignoreErrors) {
                        return callback(false, cachedListings);
                    }

                    var sellPrice = calculateSellPriceBeforeFees(null, histogram, false, 0, 65535);

                    var itemPrice = sellPrice == 65535 ?
                        '∞' :
                        (market.getPriceIncludingFees(sellPrice) / 100.0).toFixed(2) + currencySymbol;

                    var elementName = (currentPage == PAGE_TRADEOFFER ? '#item' : '#') +
                        item.appid +
                        '_' +
                        item.contextid +
                        '_' +
                        item.id;
                    var element = $(elementName);

                    $('.inventory_item_price', element).remove();
                    element.append('<span class="inventory_item_price price_' + (sellPrice == 65535 ? 0 : market.getPriceIncludingFees(sellPrice)) + '">' + itemPrice + '</span>');

                    return callback(true, cachedListings);
                });
        }
    }
    //#endregion

    //#region Market
    if (currentPage == PAGE_MARKET || currentPage == PAGE_MARKET_LISTING) {
        var marketListingsRelistedAssets = [];

        var marketListingsQueue = async.queue(function(listing, next) {
            marketListingsQueueWorker(listing,
                false,
                function(success, cached) {
                    if (success) {
                        setTimeout(function() {
                                next();
                            },
                            cached ? 0 : getRandomInt(1000, 1500));
                    } else {
                        setTimeout(function() {
                                marketListingsQueueWorker(listing,
                                    true,
                                    function(success, cached) {
                                        next(); // Go to the next queue item, regardless of success.
                                    });
                            },
                            cached ? 0 : getRandomInt(30000, 45000));
                    }
                });
        }, 1);

        marketListingsQueue.drain = function() {
            injectJs(function() {
                g_bMarketWindowHidden = false;
            })
        };

        // Gets the price, in cents, from a market listing.
        function getPriceFromMarketListing(listing) {
            var priceLabel = listing.trim().replace('--', '00');

            // Fixes RUB, which has a dot at the end.
            if (priceLabel[priceLabel.length - 1] === '.' || priceLabel[priceLabel.length - 1] === ",")
                priceLabel = priceLabel.slice(0, -1);

            // For round numbers (e.g., 100 EUR).
            if (priceLabel.indexOf('.') === -1 && priceLabel.indexOf(',') === -1) {
                priceLabel = priceLabel + ',00';
            }

            return parseInt(replaceNonNumbers(priceLabel));
        }

        function marketListingsQueueWorker(listing, ignoreErrors, callback) {
            var asset = unsafeWindow.g_rgAssets[listing.appid][listing.contextid][listing.assetid];

            // An asset:
            //{
            // "currency" : 0,
            // "appid" : 753,
            // "contextid" : "6",
            // "id" : "4363079664",
            // "classid" : "2228526061",
            // "instanceid" : "0",
            // "amount" : "1",
            // "status" : 2,
            // "original_amount" : "1",
            // "background_color" : "",
            // "icon_url" : "xx",
            // "icon_url_large" : "xxx",
            // "descriptions" : [{
            //   "value" : "Their dense, shaggy fur conceals the presence of swams of moogamites, purple scaly skin, and more nipples than one would expect."
            //  }
            // ],
            // "tradable" : 1,
            // "owner_actions" : [{
            //   "link" : "http://steamcommunity.com/my/gamecards/443880/",
            //   "name" : "View badge progress"
            //  }, {
            //   "link" : "javascript:GetGooValue( '%contextid%', '%assetid%', 443880, 7, 0 )",
            //   "name" : "Turn into Gems..."
            //  }
            // ],
            // "name" : "Wook",
            // "type" : "Loot Rascals Trading Card",
            // "market_name" : "Wook",
            // "market_hash_name" : "443880-Wook",
            // "market_fee_app" : 443880,
            // "commodity" : 1,
            // "market_tradable_restriction" : 7,
            // "market_marketable_restriction" : 7,
            // "marketable" : 1,
            // "app_icon" : "xxxx",
            // "owner" : 0
            //}

            var market_hash_name = getMarketHashName(asset);
            var appid = listing.appid;

            var listingUI = $(getListingFromLists(listing.listingid).elm);

            var game_name = asset.type;
            var price = getPriceFromMarketListing($('.market_listing_price > span:nth-child(1) > span:nth-child(1)', listingUI).text());

            if (price <= getSettingWithDefault(SETTING_PRICE_MIN_CHECK_PRICE) * 100) {
                $('.market_listing_my_price', listingUI).last().css('background', COLOR_PRICE_NOT_CHECKED);
                $('.market_listing_my_price', listingUI).last().prop('title', 'The price is not checked.');
                listingUI.addClass('not_checked');

                return callback(true, true);
            }

            var priceInfo = getPriceInformationFromItem(asset);
            var item = {
                appid: parseInt(appid),
                description: {
                    market_hash_name: market_hash_name
                }
            };

            var failed = 0;

            market.getPriceHistory(item,
                true,
                function(errorPriceHistory, history, cachedHistory) {
                    if (errorPriceHistory) {
                        logConsole('无法获取 ' + game_name + ' 的价格历史');

                        if (errorPriceHistory == ERROR_FAILED)
                            failed += 1;
                    }

                    market.getItemOrdersHistogram(item,
                        true,
                        function(errorHistogram, histogram, cachedListings) {
                            if (errorHistogram) {
                                logConsole('无法获取 '+ game_name + ' 的订单历史直方图');

                                if (errorHistogram == ERROR_FAILED)
                                    failed += 1;
                            }

                            if (failed > 0 && !ignoreErrors) {
                                return callback(false, cachedHistory && cachedListings);
                            }

                            // Shows the highest buy order price on the market listings.
                            // The 'histogram.highest_buy_order' is not reliable as Steam is caching this value, but it gives some idea for older titles/listings.
                            var highestBuyOrderPrice = (histogram == null || histogram.highest_buy_order == null ?
                                '-' :
                                ((histogram.highest_buy_order / 100) + currencySymbol));
                            $('.market_table_value > span:nth-child(1) > span:nth-child(1) > span:nth-child(1)',
                                listingUI).append(' ➤ <span title="这可能是当前最高买价。">' +
                                highestBuyOrderPrice +
                                '</span>');

                            logConsole('============================')
                            logConsole(JSON.stringify(listing));
                            logConsole(game_name + ': ' + asset.name);
                            logConsole('当前价格：' + price / 100.0);

                            // Calculate two prices here, one without the offset and one with the offset.
                            // The price without the offset is required to not relist the item constantly when you have the lowest price (i.e., with a negative offset).
                            // The price with the offset should be used for relisting so it will still apply the user-set offset.

                            var sellPriceWithoutOffset = calculateSellPriceBeforeFees(history,
                                histogram,
                                false,
                                priceInfo.minPriceBeforeFees,
                                priceInfo.maxPriceBeforeFees);
                            var sellPriceWithOffset = calculateSellPriceBeforeFees(history,
                                histogram,
                                true,
                                priceInfo.minPriceBeforeFees,
                                priceInfo.maxPriceBeforeFees);

                            var sellPriceWithoutOffsetWithFees = market.getPriceIncludingFees(sellPriceWithoutOffset);

                            logConsole('计算出的价格：' +
                                sellPriceWithoutOffsetWithFees / 100.0 +
                                ' (' +
                                sellPriceWithoutOffset / 100.0 +
                                ')');

                            listingUI.addClass('price_' + sellPriceWithOffset);

                            $('.market_listing_my_price', listingUI).last().prop('title',
                                '最好的价格是 ' + (sellPriceWithoutOffsetWithFees / 100.0) + currencySymbol + '.');

                            if (sellPriceWithoutOffsetWithFees < price) {
                                logConsole('售价太高。');

                                $('.market_listing_my_price', listingUI).last()
                                    .css('background', COLOR_PRICE_EXPENSIVE);
                                listingUI.addClass('overpriced');

                                if (getSettingWithDefault(SETTING_RELIST_AUTOMATICALLY) == 1) {
                                    queueOverpricedItemListing(listing.listingid);
                                }
                            } else if (sellPriceWithoutOffsetWithFees > price) {
                                logConsole('售价太低。');

                                $('.market_listing_my_price', listingUI).last().css('background', COLOR_PRICE_CHEAP);
                                listingUI.addClass('underpriced');
                            } else {
                                logConsole('售价正好。');

                                $('.market_listing_my_price', listingUI).last().css('background', COLOR_PRICE_FAIR);
                                listingUI.addClass('fair');
                            }

                            return callback(true, cachedHistory && cachedListings);
                        });
                });
        }

        var marketOverpricedQueue = async.queue(function(item, next) {
                marketOverpricedQueueWorker(item,
                    false,
                    function(success) {
                        if (success) {
                            setTimeout(function() {
                                    next();
                                },
                                getRandomInt(1000, 1500));
                        } else {
                            setTimeout(function() {
                                    marketOverpricedQueueWorker(item,
                                        true,
                                        function(success) {
                                            next(); // Go to the next queue item, regardless of success.
                                        });
                                },
                                getRandomInt(30000, 45000));
                        }
                    });
            },
            1);

        function marketOverpricedQueueWorker(item, ignoreErrors, callback) {
            var listingUI = getListingFromLists(item.listing).elm;

            market.removeListing(item.listing,
                function(errorRemove, data) {
                    if (!errorRemove) {
                        $('.actual_content', listingUI).css('background', COLOR_PENDING);

                        setTimeout(function() {
                            var baseUrl = $('.header_notification_items').first().attr('href') + 'json/';
                            var itemName = $('.market_listing_item_name_link', listingUI).first().attr('href');
                            var marketHashNameIndex = itemName.lastIndexOf('/') + 1;
                            var marketHashName = itemName.substring(marketHashNameIndex);
                            var decodedMarketHashName = decodeURIComponent(itemName.substring(marketHashNameIndex));
                            var newAssetId = -1;

                            unsafeWindow.RequestFullInventory(baseUrl + item.appid + "/" + item.contextid + "/", {}, null, null, function(transport) {
                                if (transport.responseJSON && transport.responseJSON.success) {
                                    var inventory = transport.responseJSON.rgInventory;

                                    for (var child in inventory) {
                                        if (marketListingsRelistedAssets.indexOf(child) == -1 && inventory[child].appid == item.appid && (inventory[child].market_hash_name == decodedMarketHashName || inventory[child].market_hash_name == marketHashName)) {
                                            newAssetId = child;
                                            break;
                                        }
                                    }

                                    if (newAssetId == -1) {
                                        $('.actual_content', listingUI).css('background', COLOR_ERROR);
                                        return callback(false);
                                    }

                                    item.assetid = newAssetId;
                                    marketListingsRelistedAssets.push(newAssetId);

                                    market.sellItem(item,
                                        item.sellPrice,
                                        function(errorSell) {
                                            if (!errorSell) {
                                                $('.actual_content', listingUI).css('background', COLOR_SUCCESS);

                                                setTimeout(function() {
                                                    removeListingFromLists(item.listing)
                                                }, 3000);

                                                return callback(true);
                                            } else {
                                                $('.actual_content', listingUI).css('background', COLOR_ERROR);
                                                return callback(false);
                                            }
                                        });

                                } else {
                                    $('.actual_content', listingUI).css('background', COLOR_ERROR);
                                    return callback(false);
                                }
                            });
                        }, getRandomInt(1500, 2500)); // Wait a little to make sure the item is returned to inventory.
                    } else {
                        $('.actual_content', listingUI).css('background', COLOR_ERROR);
                        return callback(false);
                    }
                });
        }

        // Queue an overpriced item listing to be relisted.
        function queueOverpricedItemListing(listingid) {
            var assetInfo = getAssetInfoFromListingId(listingid);
            var listingUI = $(getListingFromLists(listingid).elm);
            var price = -1;

            var items = $(listingUI).attr('class').split(' ');
            for (var i in items) {
                if (items[i].toString().includes('price_'))
                    price = parseInt(items[i].toString().replace('price_', ''));
            }

            if (price > 0) {
                marketOverpricedQueue.push({
                    listing: listingid,
                    assetid: assetInfo.assetid,
                    contextid: assetInfo.contextid,
                    appid: assetInfo.appid,
                    sellPrice: price
                });
            }
        }

        var marketRemoveQueue = async.queue(function(listingid, next) {
                marketRemoveQueueWorker(listingid,
                    false,
                    function(success) {
                        if (success) {
                            setTimeout(function() {
                                    next();
                                },
                                getRandomInt(50, 100));
                        } else {
                            setTimeout(function() {
                                    marketRemoveQueueWorker(listingid,
                                        true,
                                        function(success) {
                                            next(); // Go to the next queue item, regardless of success.
                                        });
                                },
                                getRandomInt(30000, 45000));
                        }
                    });
            },
            10);

        function marketRemoveQueueWorker(listingid, ignoreErrors, callback) {
            var listingUI = getListingFromLists(listingid).elm;

            market.removeListing(listingid,
                function(errorRemove, data) {
                    if (!errorRemove) {
                        $('.actual_content', listingUI).css('background', COLOR_SUCCESS);

                        setTimeout(function() {
                                removeListingFromLists(listingid);

                                var numberOfListings = marketLists[0].size;
                                if (numberOfListings > 0) {
                                    $('#my_market_selllistings_number').text((numberOfListings).toString());

                                    // This seems identical to the number of sell listings.
                                    $('#my_market_activelistings_number').text((numberOfListings).toString());
                                }
                            },
                            3000);

                        return callback(true);
                    } else {
                        $('.actual_content', listingUI).css('background', COLOR_ERROR);

                        return callback(false);
                    }
                });
        }

        var marketListingsItemsQueue = async.queue(function(listing, next) {
                $.get(window.location.protocol + '//steamcommunity.com/market/mylistings?count=100&start=' + listing,
                        function(data) {
                            if (!data || !data.success) {
                                next();
                                return;
                            }

                            var myMarketListings = $('#tabContentsMyActiveMarketListingsRows');

                            var nodes = $.parseHTML(data.results_html);
                            var rows = $('.market_listing_row', nodes);
                            myMarketListings.append(rows);

                            // g_rgAssets
                            unsafeWindow.MergeWithAssetArray(data.assets); // This is a method from Steam.

                            next();
                        },
                        'json')
                    .fail(function(data) {
                        next();
                        return;
                    });
            },
            1);

        marketListingsItemsQueue.drain = function() {
            var myMarketListings = $('#tabContentsMyActiveMarketListingsRows');
            myMarketListings.checkboxes('range', true);

            // Sometimes the Steam API is returning duplicate entries (especially during item listing), filter these.
            var seen = {};
            $('.market_listing_row', myMarketListings).each(function() {
                var item_id = $(this).attr('id');
                if (seen[item_id])
                    $(this).remove();
                else
                    seen[item_id] = true;

                // Remove listings awaiting confirmations, they are already listed separately.
                if ($('.item_market_action_button', this).attr('href').toLowerCase()
                    .includes('CancelMarketListingConfirmation'.toLowerCase()))
                    $(this).remove();

                // Remove buy order listings, they are already listed separately.
                if ($('.item_market_action_button', this).attr('href').toLowerCase()
                    .includes('CancelMarketBuyOrder'.toLowerCase()))
                    $(this).remove();
            });

            // Now add the market checkboxes.
            addMarketCheckboxes();

            // Show the listings again, rendering is done.
            $('#market_listings_spinner').remove();
            myMarketListings.show();

            fillMarketListingsQueue();

            injectJs(function() {
                g_bMarketWindowHidden =
                    true; // Limits the number of requests made to steam by stopping constant polling of popular listings.
            });
        };


        function fillMarketListingsQueue() {
            $('.market_home_listing_table').each(function(e) {

                // Not for popular / new / recently sold items (bottom of page).
                if ($('.my_market_header', $(this)).length == 0)
                    return;

                // Buy orders and listings confirmations are not grouped like the sell listings, add this so pagination works there as well.
                if (!$(this).attr('id')) {
                    $(this).attr('id', 'market-listing-' + e);

                    $(this).append('<div class="market_listing_see" id="market-listing-container-' + e + '"></div>')
                    $('.market_listing_row', $(this)).appendTo($('#market-listing-container-' + e));
                } else {
                    $(this).children().last().addClass("market_listing_see");
                }

                addMarketPagination($('.market_listing_see', this).last());
                sortMarketListings($(this), false, false, true);
            });

            var totalPriceBuyer = 0;
            var totalPriceSeller = 0;
            // Add the listings to the queue to be checked for the price.
            for (var i = 0; i < marketLists.length; i++) {
                for (var j = 0; j < marketLists[i].items.length; j++) {
                    var listingid = replaceNonNumbers(marketLists[i].items[j].values().market_listing_item_name);
                    var assetInfo = getAssetInfoFromListingId(listingid);

                    if (!isNaN(assetInfo.priceBuyer))
                        totalPriceBuyer += assetInfo.priceBuyer;
                    if (!isNaN(assetInfo.priceSeller))
                        totalPriceSeller += assetInfo.priceSeller;

                    marketListingsQueue.push({
                        listingid,
                        appid: assetInfo.appid,
                        contextid: assetInfo.contextid,
                        assetid: assetInfo.assetid
                    });
                }
            }

            $('#my_market_selllistings_number').append('<span id="my_market_sellistings_total_price">, ' + (totalPriceBuyer / 100.0).toFixed(2) + currencySymbol + ' ➤ ' + (totalPriceSeller / 100.0).toFixed(2) + currencySymbol + '</span>');
        }


        // Gets the asset info (appid/contextid/assetid) based on a listingid.
        function getAssetInfoFromListingId(listingid) {
            var listing = getListingFromLists(listingid);
            if (listing == null) {
                return {};
            }

            var actionButton = $('.item_market_action_button', listing.elm).attr('href');
            // Market buy orders have no asset info.
            if (actionButton == null || actionButton.toLowerCase().includes('cancelmarketbuyorder'))
                return {};

            var priceBuyer = getPriceFromMarketListing($('.market_listing_price > span:nth-child(1) > span:nth-child(1)', listing.elm).text());
            var priceSeller = getPriceFromMarketListing($('.market_listing_price > span:nth-child(1) > span:nth-child(3)', listing.elm).text());
            var itemIds = actionButton.split(',');
            var appid = replaceNonNumbers(itemIds[2]);
            var contextid = replaceNonNumbers(itemIds[3]);
            var assetid = replaceNonNumbers(itemIds[4]);
            return {
                appid,
                contextid,
                assetid,
                priceBuyer,
                priceSeller
            };
        }

        // Adds pagination and search options to the market item listings.
        function addMarketPagination(market_listing_see) {
            market_listing_see.addClass('list');

            market_listing_see.before('<ul class="paginationTop pagination"></ul>');
            market_listing_see.after('<ul class="paginationBottom pagination"></ul>');

            $('.market_listing_table_header', market_listing_see.parent())
                .append('<input class="search" id="market_name_search" placeholder="Search..." />');

            var options = {
                valueNames: [
                    'market_listing_game_name', 'market_listing_item_name_link', 'market_listing_price',
                    'market_listing_listed_date', {
                        name: 'market_listing_item_name',
                        attr: 'id'
                    }
                ],
                pagination: [{
                    name: "paginationTop",
                    paginationClass: "paginationTop",
                    innerWindow: 100,
                    outerWindow: 100,
                    left: 100,
                    right: 100
                }, {
                    name: "paginationBottom",
                    paginationClass: "paginationBottom",
                    innerWindow: 100,
                    outerWindow: 100,
                    left: 100,
                    right: 100
                }],
                page: parseInt(getSettingWithDefault(SETTING_MARKET_PAGE_COUNT))
            };

            var list = new List(market_listing_see.parent().attr('id'), options);
            list.on('searchComplete', updateMarketSelectAllButton);
            marketLists.push(list);
        }

        // Adds checkboxes to market listings.
        function addMarketCheckboxes() {
            $('.market_listing_row').each(function() {
                // Don't add it again, one time is enough.
                if ($('.market_listing_select', this).length == 0) {
                    $('.market_listing_cancel_button', $(this)).append('<div class="market_listing_select">' +
                        '<input type="checkbox" class="market_select_item"/>' +
                        '</div>');

                    $('.market_select_item', this).change(function(e) {
                        updateMarketSelectAllButton();
                    });
                }
            });
        }

        // Process the market listings.
        function processMarketListings() {
            addMarketCheckboxes();

            if (currentPage == PAGE_MARKET) {
                // Load the market listings.
                var currentCount = 0;
                var totalCount = 0;

                if (typeof unsafeWindow.g_oMyListings !== 'undefined' && unsafeWindow.g_oMyListings != null && unsafeWindow.g_oMyListings.m_cTotalCount != null)
                    totalCount = unsafeWindow.g_oMyListings.m_cTotalCount;
                else {
                    totalCount = parseInt($('#my_market_selllistings_number').text());
                }

                if (isNaN(totalCount) || totalCount == 0) {
                    fillMarketListingsQueue();
                    return;
                }

                $('#tabContentsMyActiveMarketListingsRows').html(''); // Clear the default listings.
                $('#tabContentsMyActiveMarketListingsRows').hide(); // Hide all listings until everything has been loaded.

                // Hide Steam's paging controls.
                $('#tabContentsMyActiveMarketListings_ctn').hide();
                $('.market_pagesize_options').hide();

                // Show the spinner so the user knows that something is going on.
                $('.my_market_header').eq(0).append('<div id="market_listings_spinner">' +
                    spinnerBlock +
                    '<div style="text-align:center">正在加载交易列表...</div>' +
                    '</div>');

                while (currentCount < totalCount) {
                    marketListingsItemsQueue.push(currentCount);
                    currentCount += 100;
                }
            } else {
                // This is on a market item page.
                $('.market_home_listing_table').each(function(e) {
                    // Not on 'x requests to buy at y,yy or lower'.
                    if ($('#market_buyorder_info_show_details', $(this)).length > 0)
                        return;

                    $(this).children().last().addClass("market_listing_see");

                    addMarketPagination($('.market_listing_see', this).last());
                    sortMarketListings($(this), false, false, true);
                });

                $('#tabContentsMyActiveMarketListingsRows > .market_listing_row').each(function() {
                    var listingid = $(this).attr('id').replace('mylisting_', '').replace('mybuyorder_', '').replace('mbuyorder_', '');
                    var assetInfo = getAssetInfoFromListingId(listingid);

                    // There's only one item in the g_rgAssets on a market listing page.
                    var existingAsset = null;
                    for (var appid in unsafeWindow.g_rgAssets) {
                        for (var contextid in unsafeWindow.g_rgAssets[appid]) {
                            for (var assetid in unsafeWindow.g_rgAssets[appid][contextid]) {
                                existingAsset = unsafeWindow.g_rgAssets[appid][contextid][assetid];
                                break;
                            }
                        }
                    }

                    // appid and contextid are identical, only the assetid is different for each asset.
                    unsafeWindow.g_rgAssets[appid][contextid][assetInfo.assetid] = existingAsset;
                    marketListingsQueue.push({
                        listingid,
                        appid: assetInfo.appid,
                        contextid: assetInfo.contextid,
                        assetid: assetInfo.assetid
                    });
                })
            }
        }

        // Update the select/deselect all button on the market.
        function updateMarketSelectAllButton() {
            $('.market_listing_buttons').each(function() {
                var selectionGroup = $(this).parent().parent();
                var invert = $('.market_select_item:checked', selectionGroup).length == $('.market_select_item', selectionGroup).length;
                if ($('.market_select_item', selectionGroup).length == 0) // If there are no items to select, keep it at Select all.
                    invert = false;
                $('.select_all > span', selectionGroup).text(invert ? '取消所选物品' : '选中全部物品');
            });
        }

        // Sort the market listings.
        function sortMarketListings(elem, isPrice, isDate, isName) {
            var list = getListFromContainer(elem);
            if (list == null) {
                console.log('无效参数，找不到匹配元素的列表。');
                return;
            }

            // Change sort order (asc/desc).
            var nextSort = isPrice ? 1 : (isDate ? 2 : 3);
            var asc = true;

            // (Re)set the asc/desc arrows.
            const arrow_down = '🡻';
            const arrow_up = '🡹';

            $('.market_listing_table_header > span', elem).each(function() {
                if ($(this).hasClass('market_listing_edit_buttons'))
                    return;

                if ($(this).text().includes(arrow_up))
                    asc = false;

                $(this).text($(this).text().replace(' ' + arrow_down, '').replace(' ' + arrow_up, ''));
            })

            var market_listing_selector;
            if (isPrice) {
                market_listing_selector = $('.market_listing_table_header', elem).children().eq(1);
            } else if (isDate) {
                market_listing_selector = $('.market_listing_table_header', elem).children().eq(2);
            } else if (isName) {
                market_listing_selector = $('.market_listing_table_header', elem).children().eq(3);
            }
            market_listing_selector.text(market_listing_selector.text() + ' ' + (asc ? arrow_up : arrow_down));

            if (list.sort == null)
                return;

            if (isName) {
                list.sort('', {
                    order: asc ? "asc" : "desc",
                    sortFunction: function(a, b) {
                        if (a.values().market_listing_game_name.toLowerCase()
                            .localeCompare(b.values().market_listing_game_name.toLowerCase()) ==
                            0) {
                            return a.values().market_listing_item_name_link.toLowerCase()
                                .localeCompare(b.values().market_listing_item_name_link.toLowerCase());
                        }
                        return a.values().market_listing_game_name.toLowerCase()
                            .localeCompare(b.values().market_listing_game_name.toLowerCase());
                    }
                });
            } else if (isDate) {
                var currentMonth = DateTime.local().month;

                list.sort('market_listing_listed_date', {
                    order: asc ? "asc" : "desc",
                    sortFunction: function(a, b) {
                        var firstDate = DateTime.fromString((a.values().market_listing_listed_date).trim(), 'd MMM');
                        var secondDate = DateTime.fromString((b.values().market_listing_listed_date).trim(), 'd MMM');

                        if (firstDate == null || secondDate == null) {
                            return 0;
                        }

                        if (firstDate.month > currentMonth)
                            firstDate = firstDate.plus({ years: -1});
                        if (secondDate.month > currentMonth)
                            secondDate = secondDate.plus({ years: -1});

                        if (firstDate > secondDate)
                            return 1;
                        if (firstDate === secondDate)
                            return 0;
                        return -1;
                    }
                })
            } else if (isPrice) {
                list.sort('market_listing_price', {
                    order: asc ? "asc" : "desc",
                    sortFunction: function(a, b) {
                        var listingPriceA = $(a.values().market_listing_price).text();
                        listingPriceA = listingPriceA.substr(0, listingPriceA.indexOf('('));
                        listingPriceA = listingPriceA.replace('--', '00');

                        var listingPriceB = $(b.values().market_listing_price).text();
                        listingPriceB = listingPriceB.substr(0, listingPriceB.indexOf('('));
                        listingPriceB = listingPriceB.replace('--', '00');

                        var firstPrice = parseInt(replaceNonNumbers(listingPriceA));
                        var secondPrice = parseInt(replaceNonNumbers(listingPriceB));

                        return firstPrice - secondPrice;
                    }
                })
            }
        }

        function getListFromContainer(group) {
            for (var i = 0; i < marketLists.length; i++) {
                if (group.attr('id') == $(marketLists[i].listContainer).attr('id'))
                    return marketLists[i];
            }
        }

        function getListingFromLists(listingid) {
            // Sometimes listing ids are contained in multiple lists (?), use the last one available as this is the one we're most likely interested in.
            for (var i = marketLists.length - 1; i >= 0; i--) {
                var values = marketLists[i].get("market_listing_item_name", 'mylisting_' + listingid + '_name');
                if (values != null && values.length > 0) {
                    return values[0];
                }

                values = marketLists[i].get("market_listing_item_name", 'mbuyorder_' + listingid + '_name');
                if (values != null && values.length > 0) {
                    return values[0];
                }
            }


        }

        function removeListingFromLists(listingid) {
            for (var i = 0; i < marketLists.length; i++) {
                marketLists[i].remove("market_listing_item_name", 'mylisting_' + listingid + '_name');
                marketLists[i].remove("market_listing_item_name", 'mbuyorder_' + listingid + '_name');
            }
        }

        // Initialize the market UI.
        function initializeMarketUI() {
            // Sell orders.
            $('.my_market_header').first().append(
                '<div class="market_listing_buttons">' +
                '<a class="item_market_action_button item_market_action_button_green select_all market_listing_button">' +
                '<span class="item_market_action_button_contents" style="text-transform:none">选中全部物品</span>' +
                '</a>' +
                '<span class="separator-small"></span>' +
                '<a class="item_market_action_button item_market_action_button_green remove_selected market_listing_button">' +
                '<span class="item_market_action_button_contents" style="text-transform:none">下架选中物品</span>' +
                '</a>' +
                '<a class="item_market_action_button item_market_action_button_green relist_selected market_listing_button market_listing_button_right">' +
                '<span class="item_market_action_button_contents" style="text-transform:none">重新上架选中物品</span>' +
                '</a>' +
                '<span class="separator-small"></span>' +
                '<a class="item_market_action_button item_market_action_button_green relist_overpriced market_listing_button market_listing_button_right">' +
                '<span class="item_market_action_button_contents" style="text-transform:none">重新上架高价物品</span>' +
                '</a>' +
                '<span class="separator-small"></span>' +
                '<a class="item_market_action_button item_market_action_button_green select_overpriced market_listing_button market_listing_button_right">' +
                '<span class="item_market_action_button_contents" style="text-transform:none">选中高价物品</span>' +
                '</a>' +
                '</div>');

            // Listings confirmations and buy orders.
            $('.my_market_header').slice(1).append(
                '<div class="market_listing_buttons">' +
                '<a class="item_market_action_button item_market_action_button_green select_all market_listing_button">' +
                '<span class="item_market_action_button_contents" style="text-transform:none">选中全部物品</span>' +
                '</a>' +
                '<span class="separator-large"></span>' +
                '<a class="item_market_action_button item_market_action_button_green remove_selected market_listing_button">' +
                '<span class="item_market_action_button_contents" style="text-transform:none">删除选中物品</span>' +
                '</a>' +
                '</div>');

            $('.market_listing_table_header').on('click', 'span', function() {
                if ($(this).hasClass('market_listing_edit_buttons') || $(this).hasClass('item_market_action_button_contents'))
                    return;

                var isPrice = $('.market_listing_table_header', $(this).parent().parent()).children().eq(1).text() == $(this).text();
                var isDate = $('.market_listing_table_header', $(this).parent().parent()).children().eq(2).text() == $(this).text();
                var isName = $('.market_listing_table_header', $(this).parent().parent()).children().eq(3).text() == $(this).text();

                sortMarketListings($(this).parent().parent(), isPrice, isDate, isName);
            });

            $('.select_all').on('click', '*', function() {
                var selectionGroup = $(this).parent().parent().parent().parent();
                var marketList = getListFromContainer(selectionGroup);

                var invert = $('.market_select_item:checked', selectionGroup).length == $('.market_select_item', selectionGroup).length;

                for (var i = 0; i < marketList.matchingItems.length; i++) {
                    $('.market_select_item', marketList.matchingItems[i].elm).prop('checked', !invert);
                }

                updateMarketSelectAllButton();
            });


            $('#market_removelisting_dialog_accept').on('click', '*', function() {
                // This is when a user removed an item through the Remove/Cancel button.
                // Ideally, it should remove this item from the list (instead of just the UI element which Steam does), but I'm not sure how to get the current item yet.
                window.location.reload();
            });

            $('.select_overpriced').on('click', '*', function() {
                var selectionGroup = $(this).parent().parent().parent().parent();
                var marketList = getListFromContainer(selectionGroup);

                for (var i = 0; i < marketList.matchingItems.length; i++) {
                    if ($(marketList.matchingItems[i].elm).hasClass('overpriced')) {
                        $('.market_select_item', marketList.matchingItems[i].elm).prop('checked', true);
                    }
                }

                $('.market_listing_row', selectionGroup).each(function(index) {
                    if ($(this).hasClass('overpriced'))
                        $('.market_select_item', $(this)).prop('checked', true);
                });

                updateMarketSelectAllButton();
            });

            $('.remove_selected').on('click', '*', function() {
                var selectionGroup = $(this).parent().parent().parent().parent();
                var marketList = getListFromContainer(selectionGroup);

                for (var i = 0; i < marketList.matchingItems.length; i++) {
                    if ($('.market_select_item', $(marketList.matchingItems[i].elm)).prop('checked')) {
                        var listingid = replaceNonNumbers(marketList.matchingItems[i].values().market_listing_item_name);
                        marketRemoveQueue.push(listingid);
                    }
                }
            });

            $('.market_relist_auto').change(function() {
                setSetting(SETTING_RELIST_AUTOMATICALLY, $('.market_relist_auto').is(":checked") ? 1 : 0);
            });

            $('.relist_overpriced').on('click', '*', function() {
                var selectionGroup = $(this).parent().parent().parent().parent();
                var marketList = getListFromContainer(selectionGroup);

                for (var i = 0; i < marketList.matchingItems.length; i++) {
                    if ($(marketList.matchingItems[i].elm).hasClass('overpriced')) {
                        var listingid = replaceNonNumbers(marketList.matchingItems[i].values().market_listing_item_name);
                        queueOverpricedItemListing(listingid);
                    }
                }
            });

            $('.relist_selected').on('click', '*', function() {
                var selectionGroup = $(this).parent().parent().parent().parent();
                var marketList = getListFromContainer(selectionGroup);

                for (var i = 0; i < marketList.matchingItems.length; i++) {
                    if ($(marketList.matchingItems[i].elm).hasClass('overpriced') && $('.market_select_item', $(marketList.matchingItems[i].elm)).prop('checked')) {
                        var listingid = replaceNonNumbers(marketList.matchingItems[i].values().market_listing_item_name);
                        queueOverpricedItemListing(listingid);
                    }
                }
            });

            $('#see_settings').remove();
            $('#global_action_menu').prepend('<span id="see_settings"><a href="javascript:void(0)">⬖ SEE 设置</a></span>');
            $('#see_settings').on('click', '*', () => openSettings());

            processMarketListings();
        }
    }
    //#endregion

    //#region Tradeoffers
    if (currentPage == PAGE_TRADEOFFER) {
        // Gets the trade offer's inventory items from the active inventory.
        function getTradeOfferInventoryItems() {
            var arr = [];

            for (var child in getActiveInventory().rgChildInventories) {
                for (var key in getActiveInventory().rgChildInventories[child].rgInventory) {
                    var value = getActiveInventory().rgChildInventories[child].rgInventory[key];
                    if (typeof value === 'object') {
                        // Merges the description in the normal object, this is done to keep the layout consistent with the market page, which is also flattened.
                        Object.assign(value, value.description);
                        // Includes the id of the inventory item.
                        value['id'] = key;
                        arr.push(value);
                    }
                }
            }

            // Some inventories (e.g. BattleBlock Theater) do not have child inventories, they have just one.
            for (var key in getActiveInventory().rgInventory) {
                var value = getActiveInventory().rgInventory[key];
                if (typeof value === 'object') {
                    // Merges the description in the normal object, this is done to keep the layout consistent with the market page, which is also flattened.
                    Object.assign(value, value.description);
                    // Includes the id of the inventory item.
                    value['id'] = key;
                    arr.push(value);
                }
            }

            return arr;
        }

        function sumTradeOfferAssets(assets, user) {
            var total = {};
            var totalPrice = 0;
            for (var i = 0; i < assets.length; i++) {
                var rgItem = user.findAsset(assets[i].appid, assets[i].contextid, assets[i].assetid);

                var text = '';
                if (rgItem != null) {
                    if (rgItem.element) {
                        var inventoryPriceElements = $('.inventory_item_price', rgItem.element);
                        if (inventoryPriceElements.length) {
                            var firstPriceElement = inventoryPriceElements[0];
                            var classes = $(firstPriceElement).attr('class').split(' ');
                            for (var c in classes) {
                                if (classes[c].toString().includes('price_')) {
                                    var price = parseInt(classes[c].toString().replace('price_', ''));
                                    totalPrice += price;
                                }
                            }

                        }
                    }

                    if (rgItem.original_amount != null && rgItem.amount != null) {
                        var originalAmount = parseInt(rgItem.original_amount);
                        var currentAmount = parseInt(rgItem.amount);
                        var usedAmount = originalAmount - currentAmount;
                        text += usedAmount.toString() + 'x ';
                    }

                    text += rgItem.name;

                    if (rgItem.type != null && rgItem.type.length > 0) {
                        text += ' (' + rgItem.type + ')';
                    }
                } else
                    text = 'Unknown Item';

                if (text in total)
                    total[text] = total[text] + 1;
                else
                    total[text] = 1;
            }

            var sortable = [];
            for (var item in total)
                sortable.push([item, total[item]])

            sortable.sort(function(a, b) {
                return a[1] - b[1];
            }).reverse();

            var totalText = '<strong>物品数量：' + sortable.length + '，价值 ' + (totalPrice / 100).toFixed(2) + currencySymbol + '<br/><br/></strong>';

            for (var i = 0; i < sortable.length; i++) {
                totalText += sortable[i][1] + 'x ' + sortable[i][0] + '<br/>';
            }

            return totalText;
        }
    }


    var lastTradeOfferSum = 0;

    function hasLoadedAllTradeOfferItems() {
        for (var i = 0; i < unsafeWindow.g_rgCurrentTradeStatus.them.assets.length; i++) {
            var asset = UserThem.findAsset(unsafeWindow.g_rgCurrentTradeStatus.them.assets[i].appid, unsafeWindow.g_rgCurrentTradeStatus.them.assets[i].contextid, unsafeWindow.g_rgCurrentTradeStatus.them.assets[i].assetid);
            if (asset == null)
                return false;
        }
        for (var i = 0; i < unsafeWindow.g_rgCurrentTradeStatus.me.assets.length; i++) {
            var asset = UserYou.findAsset(unsafeWindow.g_rgCurrentTradeStatus.me.assets[i].appid, unsafeWindow.g_rgCurrentTradeStatus.me.assets[i].contextid, unsafeWindow.g_rgCurrentTradeStatus.me.assets[i].assetid);
            if (asset == null)
                return false;
        }
        return true;

    }

    function initializeTradeOfferUI() {
        var updateInventoryPrices = function() {
            if (getSettingWithDefault(SETTING_TRADEOFFER_PRICE_LABELS) == 1) {
                setInventoryPrices(getTradeOfferInventoryItems());
            }
        };

        var updateInventoryPricesInTrade = function() {
            var items = [];
            for (var i = 0; i < unsafeWindow.g_rgCurrentTradeStatus.them.assets.length; i++) {
                var asset = UserThem.findAsset(unsafeWindow.g_rgCurrentTradeStatus.them.assets[i].appid, unsafeWindow.g_rgCurrentTradeStatus.them.assets[i].contextid, unsafeWindow.g_rgCurrentTradeStatus.them.assets[i].assetid);
                items.push(asset);
            }
            for (var i = 0; i < unsafeWindow.g_rgCurrentTradeStatus.me.assets.length; i++) {
                var asset = UserYou.findAsset(unsafeWindow.g_rgCurrentTradeStatus.me.assets[i].appid, unsafeWindow.g_rgCurrentTradeStatus.me.assets[i].contextid, unsafeWindow.g_rgCurrentTradeStatus.me.assets[i].assetid);
                items.push(asset);
            }
            setInventoryPrices(items);
        };

        $('.trade_right > div > div > div > .trade_item_box').observe('childlist subtree', function(record) {
            if (!hasLoadedAllTradeOfferItems())
                return;

            var currentTradeOfferSum = unsafeWindow.g_rgCurrentTradeStatus.me.assets.length + unsafeWindow.g_rgCurrentTradeStatus.them.assets.length;
            if (lastTradeOfferSum != currentTradeOfferSum) {
                updateInventoryPricesInTrade();
            }

            lastTradeOfferSum = currentTradeOfferSum;

            $('#trade_offer_your_sum').remove();
            $('#trade_offer_their_sum').remove();

            var your_sum = sumTradeOfferAssets(unsafeWindow.g_rgCurrentTradeStatus.me.assets, UserYou);
            var their_sum = sumTradeOfferAssets(unsafeWindow.g_rgCurrentTradeStatus.them.assets, UserThem);

            $('div.offerheader:nth-child(1) > div:nth-child(3)').append('<div class="trade_offer_sum" id="trade_offer_your_sum">' + your_sum + '</div>');
            $('div.offerheader:nth-child(3) > div:nth-child(3)').append('<div class="trade_offer_sum" id="trade_offer_their_sum">' + their_sum + '</div>');
        });


        // Load after the inventory is loaded.
        updateInventoryPrices();

        $('#inventory_pagecontrols').observe('childlist',
            '*',
            function(record) {
                updateInventoryPrices();
            });


        // This only works with a new trade offer.
        if (!window.location.href.includes('tradeoffer/new'))
            return;

        $('#inventory_displaycontrols').append(
            '<br/>' +
            '<div class="trade_offer_buttons">' +
            '<a class="item_market_action_button item_market_action_button_green select_all" style="margin-top:1px">' +
            '<span class="item_market_action_button_contents" style="text-transform:none">选中页面中全部物品</span>' +
            '</a>' +
            '</div>');

        $('.select_all').on('click', '*', function() {
            $('.inventory_ctn:visible > .inventory_page:visible > .itemHolder:visible').delayedEach(250, function(i, it) {
                var item = it.rgItem;
                if (item.is_stackable)
                    return;

                if (!item.tradable)
                    return;

                unsafeWindow.MoveItemToTrade(it);
            });
        });
    }
    //#endregion

    //#region Settings
    function openSettings() {
        var price_options = $('<div id="price_options">' +
            '<div style="margin-bottom:6px;">' +
            '基准价格计算方式：<select class="price_option_input" style="background-color: black;color: white;border: transparent;" id="' + SETTING_PRICE_ALGORITHM + '">' +
            '<option value="1"' + (getSettingWithDefault(SETTING_PRICE_ALGORITHM) == 1 ? 'selected="selected"' : '') + '>历史均价 和 最低售价 之间的最大值</option>' +
            '<option value="2" ' + (getSettingWithDefault(SETTING_PRICE_ALGORITHM) == 2 ? 'selected="selected"' : '') + '>最低售价</option>' +
            '<option value="3" ' + (getSettingWithDefault(SETTING_PRICE_ALGORITHM) == 3 ? 'selected="selected"' : '') + '>当前 最高买入价 或 最低售价</option>' +
            '</select>' +
            '<br/>' +
            '</div>' +
            '<div style="margin-bottom:6px;">' +
            '计算多少小时内的历史均价：<input class="price_option_input" style="background-color: black;color: white;border: transparent;" type="number" step="2" id="' + SETTING_PRICE_HISTORY_HOURS + '" value=' + getSettingWithDefault(SETTING_PRICE_HISTORY_HOURS) + '>' +
            '</div>' +
            '<div style="margin-bottom:6px;">' +
            '价格补正（基于“基准价格”在批量出售时进行调价，可为负数）；<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_PRICE_OFFSET + '" value=' + getSettingWithDefault(SETTING_PRICE_OFFSET) + '>' +
            '<br/>' +
            '</div>' +
            '<div style="margin-top:6px">' +
            '在当前最低售价较少时，使用第二低售价：<input class="price_option_input" style="background-color: black;color: white;border: transparent;" type="checkbox" id="' + SETTING_PRICE_IGNORE_LOWEST_Q + '" ' + (getSettingWithDefault(SETTING_PRICE_IGNORE_LOWEST_Q) == 1 ? 'checked=""' : '') + '>' +
            '<br/>' +
            '</div>' +
            '<div style="margin-top:6px;">' +
            '不检查指定价格及以下的市场列表：<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_PRICE_MIN_CHECK_PRICE + '" value=' + getSettingWithDefault(SETTING_PRICE_MIN_CHECK_PRICE) + '>' +
            '<br/>' +
            '</div>' +
            '<div style="margin-top:24px">' +
            '在库存中显示价格标签：<input class="price_option_input" style="background-color: black;color: white;border: transparent;" type="checkbox" id="' + SETTING_INVENTORY_PRICE_LABELS + '" ' + (getSettingWithDefault(SETTING_INVENTORY_PRICE_LABELS) == 1 ? 'checked=""' : '') + '>' +
            '</div>' +
            '<div style="margin-top:6px">' +
            '在交易报价中显示价格标签：<input class="price_option_input" style="background-color: black;color: white;border: transparent;" type="checkbox" id="' + SETTING_TRADEOFFER_PRICE_LABELS + '" ' + (getSettingWithDefault(SETTING_TRADEOFFER_PRICE_LABELS) == 1 ? 'checked=""' : '') + '>' +
            '</div>' +
            '<div style="margin-top:24px">' +
            '<div style="margin-bottom:6px;">' +
            '最低售价：<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_MIN_NORMAL_PRICE + '" value=' + getSettingWithDefault(SETTING_MIN_NORMAL_PRICE) + '>&nbsp;' +
            '最高售价：<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_MAX_NORMAL_PRICE + '" value=' + getSettingWithDefault(SETTING_MAX_NORMAL_PRICE) + '>&nbsp;普通卡牌的价格' +
            '<br/>' +
            '</div>' +
            '<div style="margin-bottom:6px;">' +
            '最低售价：<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_MIN_FOIL_PRICE + '" value=' + getSettingWithDefault(SETTING_MIN_FOIL_PRICE) + '>&nbsp;' +
            '最高售价：<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_MAX_FOIL_PRICE + '" value=' + getSettingWithDefault(SETTING_MAX_FOIL_PRICE) + '>&nbsp;闪亮卡牌的价格' +
            '<br/>' +
            '</div>' +
            '<div style="margin-bottom:6px;">' +
            '最低售价：<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_MIN_MISC_PRICE + '" value=' + getSettingWithDefault(SETTING_MIN_MISC_PRICE) + '>&nbsp;' +
            '最高售价：<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_MAX_MISC_PRICE + '" value=' + getSettingWithDefault(SETTING_MAX_MISC_PRICE) + '>&nbsp;其他物品的价格' +
            '<br/>' +
            '</div>' +
            '<div style="margin-top:24px;margin-bottom:6px;">' +
            '市场中每页的物品数量：<input class="price_option_input price_option_price" style="background-color: black;color: white;border: transparent;" type="number" step="0.01" id="' + SETTING_MARKET_PAGE_COUNT + '" value=' + getSettingWithDefault(SETTING_MARKET_PAGE_COUNT) + '>' +
            '<br/>' +
            '<div style="margin-top:6px;">' +
            '自动上架定价高于市场的物品（当库存较多时，耗时较高）：<input id="' + SETTING_RELIST_AUTOMATICALLY + '" class="market_relist_auto" type="checkbox" ' + (getSettingWithDefault(SETTING_RELIST_AUTOMATICALLY) == 1 ? 'checked=""' : '') + '>' +
            '</label>' +
            '</div>' +
            '</div>' +
            '</div>');

        var dialog = unsafeWindow.ShowConfirmDialog('Steam Economy Enhancer', price_options).done(function() {
            setSetting(SETTING_MIN_NORMAL_PRICE, $('#' + SETTING_MIN_NORMAL_PRICE, price_options).val());
            setSetting(SETTING_MAX_NORMAL_PRICE, $('#' + SETTING_MAX_NORMAL_PRICE, price_options).val());
            setSetting(SETTING_MIN_FOIL_PRICE, $('#' + SETTING_MIN_FOIL_PRICE, price_options).val());
            setSetting(SETTING_MAX_FOIL_PRICE, $('#' + SETTING_MAX_FOIL_PRICE, price_options).val());
            setSetting(SETTING_MIN_MISC_PRICE, $('#' + SETTING_MIN_MISC_PRICE, price_options).val());
            setSetting(SETTING_MAX_MISC_PRICE, $('#' + SETTING_MAX_MISC_PRICE, price_options).val());
            setSetting(SETTING_PRICE_OFFSET, $('#' + SETTING_PRICE_OFFSET, price_options).val());
            setSetting(SETTING_PRICE_MIN_CHECK_PRICE, $('#' + SETTING_PRICE_MIN_CHECK_PRICE, price_options).val());
            setSetting(SETTING_PRICE_ALGORITHM, $('#' + SETTING_PRICE_ALGORITHM, price_options).val());
            setSetting(SETTING_PRICE_IGNORE_LOWEST_Q, $('#' + SETTING_PRICE_IGNORE_LOWEST_Q, price_options).prop('checked') ? 1 : 0);
            setSetting(SETTING_PRICE_HISTORY_HOURS, $('#' + SETTING_PRICE_HISTORY_HOURS, price_options).val());
            setSetting(SETTING_MARKET_PAGE_COUNT, $('#' + SETTING_MARKET_PAGE_COUNT, price_options).val());
            setSetting(SETTING_RELIST_AUTOMATICALLY, $('#' + SETTING_RELIST_AUTOMATICALLY, price_options).prop('checked') ? 1 : 0);
            setSetting(SETTING_INVENTORY_PRICE_LABELS, $('#' + SETTING_INVENTORY_PRICE_LABELS, price_options).prop('checked') ? 1 : 0);
            setSetting(SETTING_TRADEOFFER_PRICE_LABELS, $('#' + SETTING_TRADEOFFER_PRICE_LABELS, price_options).prop('checked') ? 1 : 0);

            window.location.reload();
        });
    }
    //#endregion

    //#region UI
    injectCss('.ui-selected { outline: 2px dashed #FFFFFF; } ' +
        '#logger { color: #767676; font-size: 12px;margin-top:16px; max-height: 200px; overflow-y: auto; }' +
        '.trade_offer_sum { color: #767676; font-size: 12px;margin-top:8px; }' +
        '.trade_offer_buttons { margin-top: 12px; }' +
        '.market_commodity_orders_table { font-size:12px; font-family: "Motiva Sans", Sans-serif; font-weight: 300; }' +
        '.market_commodity_orders_table th { padding-left: 10px; }' +
        '#listings_group { display: flex; justify-content: space-between; margin-bottom: 8px; }' +
        '#listings_sell { text-align: right; color: #589328; font-weight:600; }' +
        '#listings_buy { text-align: right; color: #589328; font-weight:600; }' +
        '.market_listing_my_price { height: 50px; padding-right:6px; }' +
        '.market_listing_edit_buttons.actual_content { width:276px; transition-property: background-color, border-color; transition-timing-function: linear; transition-duration: 0.5s;}' +
        '.market_listing_buttons { margin-top: 6px; background: rgba(0, 0, 0, 0.4); padding: 5px 0px 1px 0px; }' +
        '.market_listing_button { margin-right: 4px; }' +
        '.market_listing_button_right { float:right; }' +
        '.market_listing_button:first-child { margin-left: 4px; }' +
        '.market_listing_label_right { float:right; font-size:12px; margin-top:1px; }' +
        '.market_listing_select { position: absolute; top: 16px;right: 10px; display: flex; }' +
        '#market_listing_relist { vertical-align: middle; position: relative; bottom: -1px; right: 2px; }' +
        '.pick_and_sell_button > a { vertical-align: middle; }' +
        '.market_relist_auto { margin-bottom: 8px;  }' +
        '.market_relist_auto_label { margin-right: 6px;  }' +
        '.quick_sell { margin-right: 4px; }' +
        '.spinner{margin:10px auto;width:50px;height:40px;text-align:center;font-size:10px;}.spinner > div{background-color:#ccc;height:100%;width:6px;display:inline-block;-webkit-animation:sk-stretchdelay 1.2s infinite ease-in-out;animation:sk-stretchdelay 1.2s infinite ease-in-out}.spinner .rect2{-webkit-animation-delay:-1.1s;animation-delay:-1.1s}.spinner .rect3{-webkit-animation-delay:-1s;animation-delay:-1s}.spinner .rect4{-webkit-animation-delay:-.9s;animation-delay:-.9s}.spinner .rect5{-webkit-animation-delay:-.8s;animation-delay:-.8s}@-webkit-keyframes sk-stretchdelay{0%,40%,100%{-webkit-transform:scaleY(0.4)}20%{-webkit-transform:scaleY(1.0)}}@keyframes sk-stretchdelay{0%,40%,100%{transform:scaleY(0.4);-webkit-transform:scaleY(0.4)}20%{transform:scaleY(1.0);-webkit-transform:scaleY(1.0)}}' +
        '#market_name_search { float: right; background: rgba(0, 0, 0, 0.25); color: white; border: none;height: 25px; padding-left: 6px;}' +
        '.price_option_price { width: 100px }' +
        '#see_settings { background: #26566c; margin-right: 10px; height: 24px; line-height:24px; display:inline-block; padding: 0px 6px; }' +
        '.inventory_item_price { top: 0px;position: absolute;right: 0;background: #3571a5;padding: 2px;color: white; font-size:11px; border: 1px solid #666666;}' +
        '.separator-large {display:inline-block;width:6px;}' +
        '.separator-small {display:inline-block;width:1px;}' +
        '.separator-btn-right {margin-right:12px;}' +
        '.pagination { padding-left: 0px; }' +
        '.pagination li { display:inline-block; padding: 5px 10px;background: rgba(255, 255, 255, 0.10); margin-right: 6px; border: 1px solid #666666; }' +
        '.pagination li.active { background: rgba(255, 255, 255, 0.25); }');

    $(document).ready(function() {
        // Make sure the user is logged in, there's not much we can do otherwise.
        if (!isLoggedIn) {
            return;
        }

        if (currentPage == PAGE_INVENTORY) {
            initializeInventoryUI();
        }

        if (currentPage == PAGE_MARKET || currentPage == PAGE_MARKET_LISTING) {
            initializeMarketUI();
        }

        if (currentPage == PAGE_TRADEOFFER) {
            initializeTradeOfferUI();
        }
    });

    function injectCss(css) {
        var head, style;
        head = document.getElementsByTagName('head')[0];
        if (!head) {
            return;
        }
        style = document.createElement('style');
        style.type = 'text/css';
        style.innerHTML = css;
        head.appendChild(style);
    }

    function injectJs(js) {
        var script = document.createElement('script');
        script.setAttribute("type", "application/javascript");
        script.textContent = '(' + js + ')();';
        document.body.appendChild(script);
        document.body.removeChild(script);
    }

    $.fn.delayedEach = function(timeout, callback, continuous) {
        var $els, iterator;

        $els = this;
        iterator = function(index) {
            var cur;

            if (index >= $els.length) {
                if (!continuous) {
                    return;
                }
                index = 0;
            }

            cur = $els[index];
            callback.call(cur, index, cur);

            setTimeout(function() {
                iterator(++index);
            }, timeout);
        };

        iterator(0);
    };

    String.prototype.replaceAll = function(search, replacement) {
        var target = this;
        return target.replace(new RegExp(search, 'g'), replacement);
    };
    //#endregion
})(jQuery, async);