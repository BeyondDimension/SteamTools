// ==UserScript==
// @name         steam一键移除
// @version      0.7.8
// @description  steam一键取关鉴赏家，取关游戏，清空愿望单。
// @namespace    https://greasyfork.org/users/133492
// @author       HCLonely
// @iconURL      https://store.steampowered.com/favicon.ico
// @include      *://store.steampowered.com/*
// @include      *://steamcommunity.com/*
// @supportURL   https://blog.hclonely.com/posts/6a0923b1/
// @homepage     https://blog.hclonely.com/posts/6a0923b1/
// @require      https://cdn.bootcss.com/sweetalert/2.1.2/sweetalert.min.js
// @grant        GM_xmlhttpRequest
// @grant        GM_addStyle
// @grant        GM_setValue
// @grant        GM_getValue
// @run-at       document-end
// @connect      steamcommunity.com
// @connect      steampowered.com
// ==/UserScript==

(function($) {
    'use strict';

    const url=window.location.href;
    let div=document.createElement("div");
    div.setAttribute("id", "remove");
    div.setAttribute("style", "background-color: #181f27;position:fixed;border-radius: 20px;width: 800px;height: 500px;margin: auto;top: 0;left: 0;right: 0;bottom: 0;z-index: 99999999999;display:none");
    div.innerHTML=`
<div class="button_container" style="margin: 40px 45px 15px 45px;">
    <div class="btn_wrapper">
<a id="get_fcl" href="javascript:void(0)" class="big_button">
获取鉴赏家列表					</a>
    </div>
    <div class="btn_wrapper">
<a id="get_fgl" href="javascript:void(0)" class="big_button">
获取关注游戏列表					</a>
    </div>
    <div class="btn_wrapper">
<a id="get_wl" href="javascript:void(0)" class="big_button">
获取愿望单列表					</a>
    </div>
    <div class="btn_wrapper">
<a id="unf_c" href="javascript:void(0)" class="big_button next disabled">
取关鉴赏家					</a>
    </div>
    <div class="btn_wrapper">
<a id="unf_g" href="javascript:void(0)" class="big_button next disabled">
取关游戏					</a>
    </div>
    <div class="btn_wrapper">
<a id="rem_g" href="javascript:void(0)" class="big_button next disabled">
移除愿望单					</a>
    </div>
</div>
<h2 id="pro" style="margin: 0 45px;"></h2>
<div id="output">
    <div id="setting"></div>
    <div id="info"></div>
    </div>
        <h4 class="checkbox"><input id="selectAll" type="checkbox">全选  <input id="reverse" type="checkbox">反选</p>
<a href="javascript:void(0)" style="position:absolute;top:5px;right:5px;font-size:24px;cursor:pointer" onClick="document.getElementById('remove').style.display='none'">X</a>
`;
    document.getElementsByTagName("body")[0].appendChild(div);
    let a=document.createElement("a");
    a.setAttribute("id", "remove_btn");
    a.setAttribute("class", "menuitem supernav");
    a.setAttribute("style", "cursor:pointer");
    a.innerHTML="一键移除";
    $(".supernav_container:first").append(a);

    a.onclick=function(){
        if(window.location.host=="steamcommunity.com"){
            if(confirm("此功能需要在商店页面运行，是否跳转？")) window.open("https://store.steampowered.com/","_self");
        }else if(g_AccountID==0){
            if(confirm("请先登录！")) window.open("https://store.steampowered.com/login/","_self");
        }else{
            $('#remove').show();
        }
    };
    $("#get_fcl").click(()=>{
        $("#pro").text("");
        $("#selectAll").attr("checked", false);
        $("#reverse").attr("checked", false);
        get_curators();
    });
    $("#get_fgl").click(()=>{
        $("#pro").text("");
        $("#selectAll").attr("checked", false);
        $("#reverse").attr("checked", false);
        get_follow_games();
    });
    $("#get_wl").click(()=>{
        $("#pro").text("");
        $("#selectAll").attr("checked", false);
        $("#reverse").attr("checked", false);
        get_wishlist();
    });

    $("#unf_c").click(function(){
        if(!$(this).hasClass("disabled")){
            let curators=[];
            let curatorsChecked=$("p.checkbox input:checkbox:checked");
            for(let i=0;i<curatorsChecked.length;i++){
                curators.push({"name":$(curatorsChecked[i]).attr("name"),"id":$(curatorsChecked[i]).val()});
            }
            curators.length>0?unfollow_curators(curators):alert("你还没有选中要取关的鉴赏家！");
        }
    });
    $("#unf_g").click(function(){
        if(!$(this).hasClass("disabled")){
            let games=[];
            let gamesChecked=$("p.checkbox input:checkbox:checked");
            for(let i=0;i<gamesChecked.length;i++){
                games.push({"name":$(gamesChecked[i]).attr("name"),"id":$(gamesChecked[i]).val()});
            }
            games.length>0?unfollow_games(games):alert("你还没有选中要取关的游戏！");
        }
    });
    $("#rem_g").click(function(){
        if(!$(this).hasClass("disabled")){
            let games=[];
            let gamesChecked=$("p.checkbox input:checkbox:checked");
            for(let i=0;i<gamesChecked.length;i++){
                games.push($(gamesChecked[i]).val());
            }
            games.length>0?remove_wishlist(games):alert("你还没有选中要移除愿望单的游戏！");
        }
    });

    //一键取关+移除愿望单
    if(/https?:\/\/store.steampowered.com\/app\/[\w\W]*/.test(url)){
        $("div.queue_control_button.queue_btn_ignore").after(`<div class="queue_control_button queue_btn_remove"><div class="btnv6_blue_hoverfade  btn_medium queue_btn_inactive" style="" data-tooltip-text="移除愿望单和取消关注。"><span>一键移除</span></div></div>`);
        $(".queue_btn_remove>.queue_btn_inactive").click(()=>{
            removeWishlist();
            unFollow();
        });
    }

    let [curators,unfC,unfG,remG,page,steam64ID,userName]=[[],0,0,0,1,'',''];
    userName=$('a[data-miniprofile='+g_AccountID+']').text().trim();
    let xhr = new XMLHttpRequest();
    xhr.open("GET", "https://store.steampowered.com/wishlist/id/"+userName);
    xhr.setRequestHeader('Content-Type','application/x-www-form-urlencoded; charset=UTF-8');
    xhr.onreadystatechange = function(){
        let XMLHttpReq = xhr;
        if (XMLHttpReq.readyState == 4) {
            if (XMLHttpReq.status == 200) {
                let data = XMLHttpReq.responseText;
                steam64ID = data.match(/var.*?g_strWishlistBaseURL.*?[\d]+?\\/gm)[0].match(/[\d]+/)[0];
            }else{
                swal('获取steam64位ID失败！','移除愿望单功能将不可用，其他功能可正常使用！','error');
            }
        }
    };
    xhr.send();

    //获取鉴赏家列表
    function get_curators(){
        let p=document.createElement("p");
        p.setAttribute("style", "font-size:15px");
        p.innerHTML=`获取鉴赏家列表...`;
        $("#info").append(p);
        p.scrollIntoView();

        GM_xmlhttpRequest({
            method: "GET",
            url: "https://store.steampowered.com/dynamicstore/userdata/?id="+userName+"&t="+new Date().getTime(),
            timeout: 1000*30,
            responseType: "json",
            onload: function (data) {
                if(data.status==200){
                    p.innerHTML+='<font style="color:green">成功！</font>';
                    let curator=data.response.rgCurators;
                    let curators=[];

                    let checkbox='';
                    Object.keys(curator).forEach(function(key){
                        checkbox+=`<p class="checkbox"><input type="checkbox" name="${curator[key].name}" value="${curator[key].clanid}">${curator[key].name}</p>`;
                    });
                    $("#setting").html(checkbox);

                    if(Object.keys(curator).length>0){
                        $(".next").addClass("disabled");
                        $("#unf_c").removeClass("disabled");
                    }else{
                        p.innerHTML+="<br/>关注鉴赏家列表为空！";
                        p.scrollIntoView();
                    }
                }else{
                    p.innerHTML+='<font style="color:green">失败！</font>请刷新重试';
                }
            }
        });
    }
    //读取cookie
    function getCookie(name) {
        let arr,reg=new RegExp("(^| )"+name+"=([^;]*)(;|$)");
        if(arr=document.cookie.match(reg)){
            return unescape(arr[2]);
        }else{
            return null;
        }
    }
    //取关鉴赏家
    function unfollow_curators(curators,i=0){
        i==0?$("#pro")[0].innerHTML=`取关鉴赏家进度: <font id="ard">${i}</font> / ${curators.length}`:$("#ard")[0].innerHTML=`${i}`;

        let p=document.createElement("p");
        p.innerHTML=`取关鉴赏家<a style="cursor:pointer" href=https://store.steampowered.com/curator/${curators[i].id} target="_blank">${curators[i].name}</a>...`;
        $("#info").append(p);
        p.scrollIntoView();

        GM_xmlhttpRequest({
            method : "POST",
            url: "https://store.steampowered.com/curators/ajaxfollow",
            timeout: 1000*30,
            responseType: "json",
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            data:`clanid=${curators[i].id}&sessionid=${g_sessionID}&follow=0`,
            onload: function (data) {
                if(data.status==200){
                    if(data.response.success.success==1){
                        p.innerHTML+='<font style="color:green">成功！</font>';
                        unfC++;
                    }else{
                        p.innerHTML+='<font style="color:red">失败！</font>';
                    }
                }else{
                    p.innerHTML+='<font style="color:red">失败！</font>';
                }
                $("#ard")[0].innerHTML=`${i+1}`;
                if(i<curators.length-1){
                    i++;
                    setTimeout(()=>{unfollow_curators(curators,i)},1500);
                }else{
                    let p=document.createElement("p");
                    p.setAttribute("style", "font-size:15px");
                    p.innerHTML=`取关所有鉴赏家完成,${unfC}个鉴赏家取关成功,${curators.length-unfC}个鉴赏家取关失败!<a href="https://store.steampowered.com/curators/mycurators/" target="_blank" style="cursor:pointer">点此</a>查看结果`;
                    unfC=0;
                    $("#info").append(p);
                    p.scrollIntoView();
                }
            }
        });
    }

    //获取关注游戏列表
    function get_follow_games(){
        let p=document.createElement("p");
        p.setAttribute("style", "font-size:15px");
        p.innerHTML=`获取已关注游戏列表...`;
        $("#info").append(p);
        p.scrollIntoView();

        let url=steam64ID?("https://steamcommunity.com/profiles/"+steam64ID+"/followedgames/?t="+new Date().getTime()):("https://steamcommunity.com/id/"+userName+"/followedgames/?t="+new Date().getTime());
        GM_xmlhttpRequest({
            method: "GET",
            url: url,
            timeout: 1000*30,
            onload: function (data) {
                console.log(data);
                if(data.status==200){
                    p.innerHTML+='<font style="color:green">成功！</font>';
                    let followGame=data.responseText.match(/\<div class=\"gameListRowItemName\"\>\<a .*?\>[\w\W]*?\<\/a\>\<\/div\>/gim);
                    if(followGame&&(followGame.length>0)){
                        GM_setValue('session_id',data.responseText.match(/g_sessionID = \"(.+?)\";/)[1]);
                        let gameList=unique(followGame.map((e)=>{
                            return {'id':e.match(/app\/[\d]+?\"/gim)[0].match(/[\d]+/gim)[0],'name':e.replace(/\<div class=\"gameListRowItemName\"\>/gim,"").match(/\>[\w\W]+?\</gim)[0].replace(/\<|\>/gim,"")}
                        }));
                        let checkbox='';
                        for(let i=0;i<gameList.length;i++){
                            checkbox+=`<p class="checkbox"><input type="checkbox" name="${gameList[i].name}" value="${gameList[i].id}"><a href="https://steamcommunity.com/app/${gameList[i].id}" target="_blank">${gameList[i].name}</a></p>`;
                        }
                        $("#setting").html(checkbox);

                        $(".next").addClass("disabled");
                        $("#unf_g").removeClass("disabled");
                    }else{
                        p.innerHTML+="<br/>关注游戏列表为空！";
                        p.scrollIntoView();
                    }
                }else{
                    p.innerHTML+='<font style="color:green">失败！</font>请刷新重试';
                }
            }
        });
    }
    //取关游戏
    function unfollow_games(games,i=0){
        i==0?$("#pro")[0].innerHTML=`取关游戏进度: <font id="ard">${i}</font> / ${games.length}`:$("#ard")[0].innerHTML=`${i}`;

        let gameId=games[i].id;
        let gameName=games[i].name;
        let p=document.createElement("p");
        p.innerHTML=`取关游戏<a style="cursor:pointer" href="https://store.steampowered.com/app/${gameId}" target="_blank">${gameName}</a>...`;
        $("#info").append(p);
        p.scrollIntoView();

        GM_xmlhttpRequest({
            method : "POST",
            url: "https://steamcommunity.com/app/"+gameId+"/stopfollowing",
            timeout: 1000*30,
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            data:`sessionid=${GM_getValue('session_id')}`,
            onload: function (data) {
                if(data.status==200&&(data.responseText=='null')){
                    p.innerHTML+='<font style="color:green">成功！</font>';
                    unfG++;
                }else{
                    p.innerHTML+='<font style="color:red">失败！</font>';
                }
                $("#ard")[0].innerHTML=`${i+1}`;
                if(i<games.length-1){
                    i++;
                    setTimeout(()=>{unfollow_games(games,i)},1500);
                }else{
                    let p=document.createElement("p");
                    p.setAttribute("style", "font-size:15px");
                    p.innerHTML=`取关所有游戏完成,${unfG}个游戏取关成功,${games.length-unfG}个游戏取关失败!<a href="https://steamcommunity.com/id/${userName}/followedgames/" target="_blank" style="cursor:pointer">点此</a>查看结果`;
                    unfG=0;
                    $("#info").append(p);
                    p.scrollIntoView();
                }
            }
        });
    }

    //获取愿望单列表
    function get_wishlist(){
        let p=document.createElement("p");
        p.setAttribute("style", "font-size:15px");
        p.innerHTML=`获取愿望单列表...`;
        $("#info").append(p);
        p.scrollIntoView();

        GM_xmlhttpRequest({
            method: "GET",
            url: "https://store.steampowered.com/dynamicstore/userdata/?id="+userName+"&t="+new Date().getTime(),
            timeout: 1000*30,
            responseType: "json",
            onload: function (data) {
                //console.log(data);
                if(data.status==200){
                    let wishlistGame=data.response.rgWishlist;
                    let checkbox='';
                    for(let i=0;i<wishlistGame.length;i++){
                        checkbox+=`<p class="checkbox"><input type="checkbox" name="${wishlistGame[i]}" value="${wishlistGame[i]}"><a href="https://steamcommunity.com/app/${wishlistGame[i]}" target="_blank">${wishlistGame[i]}</a></p>`;
                    }
                    $("#setting").html(checkbox);
                    p.innerHTML+='<font style="color:green">成功！</font>';

                    if(wishlistGame.length>0){
                        $(".next").addClass("disabled");
                        $("#rem_g").removeClass("disabled");
                    }else{
                        p.innerHTML+="<br/>愿望单为空！";
                        p.scrollIntoView();
                    }
                }else{
                    p.innerHTML+='<font style="color:green">失败！</font>请刷新重试';
                }
            }
        });
    }
    //移除愿望单
    function remove_wishlist(wishlist,i=0){
        i==0?$("#pro")[0].innerHTML=`取关游戏进度: <font id="ard">${i}</font> / ${wishlist.length}`:$("#ard")[0].innerHTML=`${i}`;

        let gameId=wishlist[i];
        let p=document.createElement("p");
        p.innerHTML=`移除游戏<a style="cursor:pointer" href="https://store.steampowered.com/app/${gameId}" target="_blank">${gameId}</a>...`;
        document.getElementById("info").appendChild(p);
        p.scrollIntoView();

        GM_xmlhttpRequest({
            method : "POST",
            url: "https://store.steampowered.com/wishlist/profiles/"+userName+"/remove/",
            timeout: 1000*30,
            cache: false,
            responseType: "json",
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            data:`appid=${gameId}&sessionid=${g_sessionID}`,
            onload: function (data) {
                if(data.status==200){
                    if(data.response.success==1){
                        p.innerHTML+='<font style="color:green">成功！</font>';
                        remG++;
                    }else{
                        p.innerHTML+='<font style="color:red">失败！</font>';
                    }
                }else{
                    p.innerHTML+='<font style="color:red">失败！</font>';
                }
                $("#ard")[0].innerHTML=`${i+1}`;
                if(i<wishlist.length-1){
                    i++;
                    setTimeout(()=>{remove_wishlist(wishlist,i)},1500);
                }else{
                    let p=document.createElement("p");
                    p.setAttribute("style", "font-size:15px");
                    p.innerHTML=`移除愿望单游戏完成,${remG}个游戏移除成功,${wishlist.length-remG}个游戏移除失败!<a href="https://store.steampowered.com/wishlist/id/${userName}" target="_blank" style="cursor:pointer">点此</a>查看结果`;
                    remG=0;
                    $("#info").append(p);
                    p.scrollIntoView();
                }
            }
        });
    }

    //一键取关+移除愿望单
    function getAppid(){
        return url.replace("https://store.steampowered.com/app/","").match(/[\d]+?\//)[0].replace("/","");
    }
    function removeWishlist(){
        $.ajax({
            type: "post",
            url: "https://store.steampowered.com/wishlist/profiles/"+steam64ID+"/remove/",
            datatype: "json",
            cache: false,
            data:{
                sessionid:g_sessionID,
                appid:getAppid(),
            },
            crossDomain:true,
            xhrFields: {
                withCredentials: true
            },
            success: function (data) {
                if(data.success==true){
                    if($("#add_to_wishlist_area").length>0){
                        $("#add_to_wishlist_area").show();
                    }else{
                        let btn=$("a.queue_btn_active:contains('已在愿望单中')");
                        btn.removeClass("queue_btn_active");
                        btn.html("<span>已移除</span>");
                    }
                    $("#add_to_wishlist_area_success").hide();
                }
            },
        });
    }
    function unFollow(){
        $.ajax({
            type: "post",
            url: '//store.steampowered.com/explore/followgame/',
            datatype: "json",
            cache: false,
            data:{
                sessionid:g_sessionID,
                appid:getAppid(),
                unfollow: '1',
            },
            crossDomain:true,
            xhrFields: {
                withCredentials: true
            },
            success: function (data) {
                if(data==true){
                    $("div.queue_control_button.queue_btn_follow>.queue_btn_inactive").show();
                    $("div.queue_control_button.queue_btn_follow>.queue_btn_active").hide();
                }
            }
        });
    }
    $("p.checkbox input:checkbox").click(()=>{
        $("#selectAll").attr("checked", false);
    });
    $("#selectAll").click(function() {
        let thisChexk=this.checked;
        $("p.checkbox input:checkbox").each(function() {
            $(this).attr("checked", thisChexk);
        });
    });
    $("#reverse").click(function() {
        $("#selectAll").attr("checked", false);
        $("p.checkbox input:checkbox").each(function() {
            this.checked = !this.checked;
        });
    });


    //数组去重
    function unique(arr){
        return [...new Set(arr)];
    }

    GM_addStyle(`
#output {
    background-color: #1e3a4c;
    border-radius: 3px;
    border: 1px solid rgba( 0, 0, 0, 0.3);
    box-shadow: 1px 1px 0px rgba( 255, 255, 255, 0.2);
    color: #fff;
    margin: 0 55px 0 45px;
    height: 320px;
    padding: 0 5px;
}

#setting {
    width: 50%;
    height: 100%;
    position: relative;
    overflow-y: auto;
    overflow-x: hidden;
}

#info {
    position: relative;
    left: 50%;
    top: -320px;
    width: 50%;
    height: 100%;
    border-left-style: dashed;
    padding-left: 5px;
    overflow-y: auto;
    overflow-x: hidden;
}

.checkbox,#info p {
    font-size: 15px;
}

h4.checkbox {
    bottom: -10px;
    z-index: 99999;
    margin: 10px 45px;
}

.btn_wrapper {
    margin: 2px 0;
    margin-right: 8px;
    display: inline-block;
}

.disabled {
    cursor: not-allowed !important;
    background: #6b7373 !important;
    color: #8da5a5 !important;
}

.disabled:hover {
    cursor: not-allowed !important;
    background: #6b7373 !important;
    color: #8da5a5 !important;
}

.big_button {
    cursor: pointer;
    width: 226px;
    height: 29px;
    font-family: "Motiva Sans", Sans-serif;
    font-weight: 300;
    display: inline-block;
    font-size: 18px;
    line-height: 28px;
    color: #66c0f4;
    text-align: center;
    background-image: url(//steamstore-a.akamaihd.net/public/images/v6/home/background_spotlight.jpg);
    background-position-y: -105px;
    border-radius: 3px;
    box-shadow: 0 0 4px #000;
}

.queue_btn_remove {
    padding-left: 5px;
}
`);

})(jQuery);
