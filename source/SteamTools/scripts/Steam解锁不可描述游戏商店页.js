// ==UserScript==
// @name         Steam解锁不可描述游戏商店页
// @namespace    http://tampermonkey.net/
// @version      1.1
// @description  Steam Store (Turn on mature content in some region)(此脚本需要勾选Steam商店加速才会生效)
// @author       Dogfight360
// @match        http*://store.steampowered.com/
// @icon         http://store.steampowered.com/favicon.ico
// @namespace    https://greasyfork.org/
// @enable      true
// @grant        none
// ==/UserScript==

(function() {
    document.cookie="wants_mature_content=1"
    document.cookie="birthtime=22503171"
})();