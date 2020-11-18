// ==UserScript==
// @name         Steam自动探索队列
// @namespace    http://steamcn.com/t157861-1-1
// @version      0.2
// @description  Steam节庆活动用脚本，自动探索3次队列。(Автоматический просмотр трех списков рекомендаций во время распродажи)
// @author       baodongsun (перевод Volk_J)
// @match        https://store.steampowered.com/
// @grant        SteamCN
// ==/UserScript==

(function() {
    'use strict';

    // Your code here...
})();(function _exec(){
var appids,
    running = true,
    queueNumber,
    progressDialog = ShowAlertDialog('Процесс', $J('<div/>').append($J('<div/>', {'class': 'waiting_dialog_throbber'}) ).append( $J('<div/>', {'id': 'progressContainer'}).text('Загрузка...') ), 'Остановить').done(abort);
function abort(){
  running = false;
  progressDialog.Dismiss();
}
function retry(){
  abort();
  ShowConfirmDialog('Ошибка', 'Попробуйте еще раз?', 'Попробовать снова', 'Отказаться').done(_exec)
}
function clearApp(){
  if(!running)
    return;
  showProgress();
  var appid = appids.shift();
  !appid ? generateQueue() : $J.post( appids.length ? '/app/' + appid : '/explore/next/', {sessionid: g_sessionID, appid_to_clear_from_queue: appid} ).done(clearApp).fail(retry);
}
function generateQueue(){
  running && $J.post('/explore/generatenewdiscoveryqueue', {sessionid: g_sessionID, queuetype: 0}).done(beginQueue).fail(retry);
}
function beginQueue(){
  if(!running)
    return;
  $J.get('/explore/').done(function(htmlText){
    var cardInfo = htmlText.match(/<div class="subtext">\D+(\d)\D+<\/div>/);
    if( !cardInfo ){
      abort();
      ShowAlertDialog('Выполнено','Завершен просмотр 3-х списков рекомендаций');
      return;
    }
    var matchedAppids = htmlText.match(/0,\s+(\[.*\])/);
    if( !matchedAppids ){
      retry();
      return;
    }
    appids = JSON.parse(matchedAppids[1]);
    queueNumber = cardInfo[1];
    appids.length == 0 ? generateQueue() : clearApp();
    showProgress();
  })
}
function showProgress(){
  $J('#progressContainer').html( '<br> Осталось просмотреть списков рекомендаций ' + queueNumber + '<br> Осталось просмотреть игр ' + appids.length );
}
beginQueue();
}())