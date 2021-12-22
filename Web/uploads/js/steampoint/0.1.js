// ==UserScript==
// @name         Steam 点数商店个人资料背景预览
// @namespace    https://keylol.com/t673627-1-1
// @version      0.1
// @description  支持精选艺术作品展柜预览及下载
// @author       wave
// @match        http*://store.steampowered.com/points/shop*
// @match        http*://steamcommunity.com/profiles/*
// @match        http*://steamcommunity.com/id/*
// @match        http*://artwork.m0n5ter.com/*
// @grant        GM_addStyle
// @grant        GM_xmlhttpRequest
// @connect      steamcdn-a.akamaihd.net
// @connect      cdn.cloudflare.steamstatic.com
// @connect      media.st.dl.pinyuncloud.com
// @icon         https://store.steampowered.com/favicon.ico
// @run-at       document-body
// ==/UserScript==
$(document).ready(function(){
(function() {
    if (window.location.href.indexOf('store.steampowered.com/points/shop') !== -1) {
        GM_addStyle(`
            .rewarditem_Preview {
                font-size: .6rem;
                letter-spacing: 0.1em;
                margin: auto 12px auto auto;
                white-space: nowrap;
            }
            .rewarditem_Preview:hover {
                color: #78A9E2;
                -webkit-text-fill-color: #78A9E2;
            }
        `);
        let mutationObserver = new MutationObserver(function() {
            $J('[class*="rewarditem_BackgroundOverride"] [class*="rewarditem_ItemTypeContainer"]:not(:has(.rewarditem_Preview))').each(function() {
                let $BackgroundPreview = $J('<span/>', {'class': 'rewarditem_Preview'}).append('预览');
                $J(this).append($BackgroundPreview);
                $BackgroundPreview.on('click', function() {
                    let keys = Object.keys($J(this).parents('[class*="cluster_ItemHover"]')[0]);
                    let instanceKey = keys.filter(key => key.startsWith('__reactInternalInstance'))[0];
                    let instance = $J(this).parents('[class*="cluster_ItemHover"]')[0][instanceKey];
                    try {
                        let props = instance.pendingProps.children.props;
                        let config = $J('#application_config').data('config');
                        let data = {
                            animated: props.definition.community_item_data.animated,
                            title: props.definition.community_item_data.item_title,
                            image: config.MEDIA_CDN_COMMUNITY_URL + 'images/items/' + props.definition.appid + '/' + props.definition.community_item_data.item_image_large,
                            mp4: config.MEDIA_CDN_COMMUNITY_URL + 'images/items/' + props.definition.appid + '/' + props.definition.community_item_data.item_movie_mp4,
                            webm: config.MEDIA_CDN_COMMUNITY_URL + 'images/items/' + props.definition.appid + '/' + props.definition.community_item_data.item_movie_webm
                        };
                        window.open('https://steamcommunity.com/my?preview=true&' + $J.param(data));
                    } catch(err) {
                        ShowAlertDialog('错误', '无法预览，请稍后再试。', '确定');
                    }
                    return false;
                });
            });
        });
        mutationObserver.observe(document.body, {childList: true, subtree: true});
    } else if (window.location.search.indexOf('preview=true') !== -1) {
        GM_addStyle(`
            body.profile_page, div.profile_page {
                background-image: none !important;
            }
            .profile_animated_background {
                display: none;
            }
            .profile_customization:first-child .screenshot_showcase_primary.single .screenshot_showcase_screenshot img {
                display: none;
            }
        `);
        let params = new URLSearchParams(window.location.search);
        let data = {};
        for (let [key, value] of params) {
            data[key] = value;
        }
        if (data) {
            (async function() {
                let imageUrl = await new Promise(function(resolve, reject) {
                    GM_xmlhttpRequest({
                        method: 'GET',
                        url: data.image,
                        responseType: 'blob',
                        onload: function(response) {
                            resolve(window.URL.createObjectURL(response.response));
                        },
                        onerror: function() {
                            resolve(data.image);
                        }
                    });
                });
                let $ProfileAnimatedBackgroundVideo = $J('<video/>', {'playsinline': 'playsinline', 'autoplay': 'autoplay', 'muted': 'muted', 'loop': 'loop', 'poster': imageUrl});
                let $ShowcaseAnimatedBackgroundVideo = $J('<video/>', {'playsinline': 'playsinline', 'autoplay': 'autoplay', 'muted': 'muted', 'loop': 'loop', 'poster': imageUrl, 'style': 'display: block; margin-top: -256px; margin-left: -495px;'});
                let $ShowcaseBackgroundImage = $J('<img/>', {'src': imageUrl, 'style': 'display: block; margin-top: -256px; margin-left: -495px;'});
                $J(function() {
                    $J('.profile_page, .profile_content').addClass('has_profile_background');
                    $J('.profile_animated_background').remove();
                    $J('.profile_customization:first .screenshot_showcase_primary.single .screenshot_showcase_screenshot').removeAttr('href').find('img').remove();
                    if (data.animated === 'true') {
                        $J('div.profile_page').prepend($J('<div/>', {'class': 'profile_animated_background', 'style': 'display: block;'}).append($ProfileAnimatedBackgroundVideo));
                        $J('.profile_customization:first .screenshot_showcase_primary.single .screenshot_showcase_screenshot').append($ShowcaseAnimatedBackgroundVideo);
                        $ShowcaseAnimatedBackgroundVideo.on('click', function() {
                            window.open('https://artwork.m0n5ter.com/?url=' + encodeURIComponent(data.mp4));
                            return false;
                        });
                    } else {
                        $J('div.profile_page').css('cssText', 'background-image: url("' + imageUrl + '") !important');
                        $J('.profile_customization:first .screenshot_showcase_primary.single .screenshot_showcase_screenshot').append($ShowcaseBackgroundImage);
                        $ShowcaseBackgroundImage.on('click', function() {
                            let width = $J(this).parent().width();
                            let height = $J(this).parent().height();
                            let canvas = document.createElement('canvas');
                            canvas.width = width;
                            canvas.height = height;
                            let ctx = canvas.getContext('2d');
                            let img = new Image();
                            img.src = imageUrl;
                            img.onload = function() {
                                ctx.drawImage(img, 495, 256, width, height, 0, 0, width, height);
                                canvas.toBlob(function(blob) {
                                    let a = document.createElement('a');
                                    a.setAttribute('href', window.URL.createObjectURL(blob));
                                    a.setAttribute('download', data.title ? data.title : 'showcase');
                                    document.documentElement.appendChild(a);
                                    a.dispatchEvent(new MouseEvent('click'));
                                    document.documentElement.removeChild(a);
                                });
                            };
                            return false;
                        });
                    }
                });
                if (data.animated === 'true') {
                    let mp4Url = await new Promise(function(resolve, reject) {
                        GM_xmlhttpRequest({
                            method: 'GET',
                            url: data.mp4,
                            responseType: 'blob',
                            onload: function(response) {
                                resolve(window.URL.createObjectURL(response.response));
                            },
                            onerror: function() {
                                resolve(data.mp4);
                            }
                        });
                    });
                    $ProfileAnimatedBackgroundVideo.append($J('<source/>', {'src': mp4Url, 'type': 'video/mp4'}));
                    $ShowcaseAnimatedBackgroundVideo.append($J('<source/>', {'src': mp4Url, 'type': 'video/mp4'}));
                }
            })();
        }
    } else if (window.location.hostname === 'artwork.m0n5ter.com' && window.location.search.indexOf('url=') !== -1) {
        window.onload = function() {
            let params = new URLSearchParams(window.location.search);
            let url = params.get('url');
            if (url) {
                document.querySelector('#url').value = url;
                document.querySelector('#url').dispatchEvent(new Event('input', {bubbles: true}));
            }
        };
    }
})();
    });