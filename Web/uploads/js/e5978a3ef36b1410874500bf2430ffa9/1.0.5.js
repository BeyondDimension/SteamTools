// ==UserScript==
// @name         Steam解锁区域限制
// @namespace    http://tampermonkey.net/
// @version      1.0.5
// @description  Bypass some kind of region restrictions.(2.4.1 以上版本可正常使用)
// @author       CharRun
// @grant        GM_xmlhttpRequest
// @run-at       document-body
// @connect      steampowered.com
// @include      https://store.steampowered.com/app/*
// @include      https://store.steampowered.com/bundle/*
// @include      https://store.steampowered.com/sub/*
// @include      https://store.steampowered.com/search/*
// @include      https://store.steampowered.com/agecheck/*
// ==/UserScript==

(()=>{
let accountID = 0;
let currentAppID = 0;
let currentStoreType = null;
let currentCountry = null;

let conf = {
	autoCheck:true,
	advancedSearch:false,
	defaultCountry:"美国",
	presetCountriesList:{
		"中国":"cn",
		"美国":"us",
		"加拿大":"ca",
		"俄罗斯":"ru",
		"澳大利亚":"au",		
		"英国":"gb",
		"新加坡":"sg",
		"法国":"fr",
		"香港":"hk",
		"印度":"in",
		"日本":"jp",
		"韩国":"kr"
	}
};

let temp = {};
let blockingWaitDialog;

function init() {
	conf = JSON.parse(localStorage.getItem("STEAMCC_CONF")) || conf;
	if (typeof g_AccountID == "undefined" || typeof g_oSuggestParams == "undefined") {
		setTimeout(init,500);
		return;
	}
	accountID = g_AccountID;
	currentCountry = g_oSuggestParams ? g_oSuggestParams.cc.toLowerCase() : null;
}


function autoCheck() {
	if( location.pathname.match( /\/(app|sub|bundle)\/([0-9]{1,7})/ ) ){
		let result = location.pathname.match( /\/(app|sub|bundle)\/([0-9]{1,7})/ );
		currentStoreType = result[1];
		currentAppID = result[2];
		if((document.getElementById("error_box")||document.getElementById("agecheck_form")||document.getElementById("app_agegate")) && conf.autoCheck){
			if( cookieMIssCheck() ){
				blockingWaitDialog = ShowBlockingWaitDialog("检测到访问限制", `${conf.defaultCountry} 区域匿名数据获取中...`);
				anonymousAccess(conf.defaultCountry);
			}else{
				blockingWaitDialog = ShowBlockingWaitDialog("验证信息缺失", "设置关键Cookies中...");
				setCookie();			
			}
		}
	}else if( /search\//ig.test(location.pathname) ){
		currentStoreType = "search";
		if( conf.advancedSearch ) {
			blockingWaitDialog = ShowBlockingWaitDialog("跨区搜索", `${conf.defaultCountry} 区域搜索结果获取中...`);
			anonymousAccess(conf.defaultCountry);
		}
	}else{
		//Not supported yet
	}
}



function cookieMIssCheck() {
	let cookies = cookieParse();
	let wants_mature_content = cookies.wants_mature_content ? cookies.wants_mature_content == "1" ? true : false : false;
	let mature_content = cookies.mature_content ? cookies.mature_content == "1" ? true : false : false;
	let birthtime = cookies.birthtime ? cookies.birthtime <= "978278400" ? true : false : false;
	let lastagecheckage = cookies.lastagecheckage ? true : false;
	if(wants_mature_content&&mature_content&&birthtime&&lastagecheckage) return true;
	return false;
}
	
function cookieParse() {
	let cookieString = document.cookie;
	let cookieArray = cookieString.split(";");
	let cookies = {};
	for(let i = 0 , l = cookieArray.length; i < l ; i++){
		let cookie = cookieArray[i].trim();
		if(cookie == "") continue;
		let c = cookie.split("=");
		cookies[c[0]] = c[1];
	}
	return cookies;
}

function setCookie() {
	let date = new Date();
	date.setTime(date.getTime() + 1000 * 3600 * 24 * 365);
	let expires = `expires=${date.toUTCString()};`;
	let path = "path=/;";
	document.cookie= "wants_mature_content=1;" + path + expires;
	document.cookie= "mature_content=1;" +  path + expires;
	document.cookie= "lastagecheckage=1-January-1970;" +  path + expires;
	document.cookie= "birthtime=-729000000;" +  path + expires;
	blockingWaitDialog = blockingWaitDialog && blockingWaitDialog.Dismiss();
	location.href = location.href.replace( /\/agecheck/, "" );
}


function anonymousAccess(region) {
	region = region ? conf.presetCountriesList[region] ? conf.presetCountriesList[region] : region : "us";
	if(currentCountry == region){
		blockingWaitDialog = blockingWaitDialog && blockingWaitDialog.Dismiss();
		ShowConfirmDialog("访问暂停","访问区域与当前账户区域一致","继续访问","返回").done( result => {
			result && access(region);
		} );
		return;
	}else{
		access(region);		
	}

	function access(region) {
		blockingWaitDialog = blockingWaitDialog || ShowBlockingWaitDialog("匿名访问中", `${region} 区域匿名数据获取中...`);

		let url = `${location.protocol + "//store.steampowered.com" + location.pathname.replace( /\/agecheck/, "" ) + (location.search == "" ? "?" : (location.search + "&"))}cc=${region}`;

		GM_xmlhttpRequest({
			method:"GET",
			url:url,
			timeout:5000,
			headers:{
				Cookie:"wants_mature_content=1;mature_content=1;lastagecheckage=1-January-1970;birthtime=0"
			},
			onload: res => ifarme(res,region),
			ontimeout:timeout,
			onerror:fail
		});

		function timeout () {
			blockingWaitDialog = blockingWaitDialog && blockingWaitDialog.Dismiss();
			ShowConfirmDialog("访问超时","数据获取失败....","再试一次","取消").done( result => {
				result && access(region);
			});			
		}

		function fail() {
			blockingWaitDialog = blockingWaitDialog && blockingWaitDialog.Dismiss();
			ShowConfirmDialog("访问失败","数据获取失败....","再试一次","取消").done( result => {
				result && access(region);
			});
		}
	}
}


function ifarme(res,region) {
	if(/id="error_box"/ig.exec(res.response)){
		blockingWaitDialog = blockingWaitDialog && blockingWaitDialog.Dismiss();
		ShowConfirmDialog("访问失败","访问区域受限制( 如访问区域无限制请反馈 )","确认","<a href=\"https://keylol.com/t482883-1-1\" title=\"\" target=\"_blank\">反馈</a>");
		return;
	}else if( res.finalUrl == "https://store.steampowered.com/"){
		blockingWaitDialog = blockingWaitDialog && blockingWaitDialog.Dismiss();
		ShowConfirmDialog("访问失败","被重定向为主页( 如访问区域无限制请反馈 )","确认","<a href=\"https://keylol.com/t482883-1-1\" title=\"\" target=\"_blank\">反馈</a>");
		return;		
	}

	let virtualDom = document.createElement("html");
	virtualDom.innerHTML = res.response;
	document.querySelector("title").innerHTML = virtualDom.querySelector("title").innerHTML;
	virtualDom.querySelector("head").innerHTML += `<base target=\"_blank\"><script>for(let i in parent.GDynamicStore){if(typeof GDynamicStore[i]=="object"){if(i=="s_rgfnOnReadyCallbacks")continue;GDynamicStore[i]=parent.GDynamicStore[i];}};ShowAlertDialog=parent.ShowAlertDialog;</script>`;
	let hint = `<div class="game_area_already_owned page_content"style="position:relative;background: linear-gradient(to right, rgba(255, 50, 50, 0.8)  0%,rgba(255, 180,180, 1) 100%);color: white"><div class="ds_owned_flag ds_flag"style="background-color: rgb(255, 140, 0)">脚本页面&nbsp;&nbsp;</div><div class="already_in_library"style="">此页面为<a href="https://keylol.com/t482883-1-1">SteamCC脚本</a>生成</div></div>`;
	virtualDom.querySelector("body").innerHTML = hint + virtualDom.querySelector(".responsive_page_template_content").innerHTML;
	let rNode = document.getElementsByClassName("responsive_page_template_content")[0];

	if(accountID || currentStoreType == "search"){
		htmlModify(rNode,virtualDom,region);
	}
	let content = `<html class="responsive">${virtualDom.innerHTML}</html>`;

	var iframe = document.createElement("iframe");
	iframe.id = "anonymousAccessIframe";
	iframe.style.width = "100%";
	iframe.style.height = "0";
	iframe.style.border = "none";
	iframe.style.overflow = "hidden";
	iframe.scrolling = "no";
	iframe.frameborder = "0";

	rNode.innerHTML = "";
	rNode.appendChild(iframe);

	let frameWin = iframe;
	if( iframe.contentWindow ){
		frameWin = iframe.contentWindow;
	}

	frameWin.document.open();
	frameWin.document.writeln(content);
	frameWin.document.close();
	frameWin.GM_xmlhttpRequest = GM_xmlhttpRequest;
	frameWin.onload = function(){
		if( currentStoreType == "search" ){
			// fix spa histroy, BUG: can't forward;
			FillFormFromNavigation = frameWin.FillFormFromNavigation;
			OnLocationChange = frameWin.OnLocationChange;
		}
		blockingWaitDialog = blockingWaitDialog && blockingWaitDialog.Dismiss();
		function resize() {
			iframe.style.height = "auto"; 			// for get actual height
			iframe.style.height = frameWin.document.documentElement.scrollHeight + "px";
		}
		window.onresize = resize;
		let t = setInterval(() => {iframe.style.height = frameWin.document.documentElement.scrollHeight + "px";},600);
		setTimeout(()=>clearInterval(t),6000);
		virtualDom = content = null;
	};
}

function htmlModify(rNode,virtualDom,region){
	temp.foryou_tab = rNode.querySelector("#foryou_tab") ? rNode.querySelector("#foryou_tab").innerHTML : temp.foryou_tab;
	temp.foryou_flyout = rNode.querySelector("#foryou_flyout") ? rNode.querySelector("#foryou_flyout").innerHTML : temp.foryou_flyout;
	temp.foryou_tab && (virtualDom.querySelector("#foryou_tab").innerHTML = temp.foryou_tab);
	temp.foryou_flyout && (virtualDom.querySelector("#foryou_flyout").innerHTML = temp.foryou_flyout);

	switch (currentStoreType){
		case "app":
			if( document.querySelectorAll(".queue_actions_ctn a").length || (temp.block && temp.game_meta_data) ){
				temp.block = document.querySelector(".block .game_background_glow") ? document.querySelector(".block .game_background_glow").parentNode.innerHTML : temp.block;
				temp.game_meta_data = document.querySelector("#responsive_apppage_details_left_ctn") ? document.querySelector("#responsive_apppage_details_left_ctn").parentNode.innerHTML : temp.game_meta_data;
				virtualDom.querySelector(".block .game_background_glow").parentNode.innerHTML = temp.block;
				virtualDom.querySelector("#responsive_apppage_details_left_ctn").parentNode.innerHTML = temp.game_meta_data;
				
			}else {
				let wish_list_innerHTML = "";
				//add queue 
				wish_list_innerHTML += `<a href="https://store.steampowered.com/explore/"class="btnv6_blue_hoverfade  btn_medium  right"data-tooltip-text="查看和定制您个性化的探索队列。"><span>查看您的队列&nbsp;&nbsp;&nbsp;<i class="ico16 arrow_next"></i></span></a>`;

				//wishlist
				if( currentAppID in GDynamicStore.s_rgOwnedApps ){
					const ID_64 = `7656${accountID + 1197960265728}`;
					let appName = virtualDom.querySelector(".apphub_AppName").innerText;
					let parentNode = virtualDom.querySelector(".block .game_background_glow").parentNode;
					let ownApp = document.createElement("div");
					let playStats = document.createElement("div");
					ownApp.className = "game_area_already_owned page_content";
					playStats.className = "game_area_play_stats";
					ownApp.innerHTML = `<div class="ds_owned_flag ds_flag">已在库中&nbsp;&nbsp;</div><div class="already_in_library">您的Steam库中已有《${appName}》</div>`;
					playStats.innerHTML = `<div class="already_owned_actions"><!--install steam link--><div class="game_area_already_owned_btn"><a class="btnv6_lightblue_blue btnv6_border_2px btn_medium"href="https://store.steampowered.com/about/"><span>安装Steam</span></a></div><!--play link--><div class="game_area_already_owned_btn"><a class="btnv6_lightblue_blue btnv6_border_2px btn_medium"href="steam://run/${currentAppID}"><span>马上开玩</span></a></div></div><div class="block myactivity_block"id="my_activity"><div class="details_block hours_played">总时数 N/A 小时</div><div class="details_block"><a href="https://steamcommunity.com/profiles/${ID_64}/stats/appid/${currentAppID}">查看您的统计资料</a><a href="https://steamcommunity.com/stats/${currentAppID}/achievements">查看全球游戏统计资料</a></div></div><div style="clear:left;"></div>`;
					parentNode.appendChild(ownApp);
					parentNode.appendChild(playStats);
				}else{
					const ID_64 = `7656${accountID + 1197960265728}`;
					let temp = "";
					if( currentAppID in GDynamicStore.s_rgWishlist ){
						temp += `<div id="add_to_wishlist_area_success"style="display: block;"><a href="https://steamcommunity.com/profile/${ID_64}/wishlist"class="btnv6_blue_hoverfade btn_medium queue_btn_active"data-tooltip-text="该产品已在您的愿望单中。点击查看您的愿望单。"><span><img src="https://steamstore-a.akamaihd.net/public/images/v6/ico/ico_selected.png"border="0">已在愿望单中</span></a></div>`;
					}else{
						temp += `<div id="add_to_wishlist_area"><a class="btnv6_blue_hoverfade btn_medium"href="javascript:AddToWishlist( ${currentAppID}, 'add_to_wishlist_area', 'add_to_wishlist_area_success', 'add_to_wishlist_area_fail', '1_5_9__407' );"data-tooltip-text="在您愿望单中的物品正式发布或特价销售时获取邮件通知"><span>添加至您的愿望单</span></a></div><div id="add_to_wishlist_area_success"style="display: none;"><a href="https://steamcommunity.com/profile/${ID_64}/wishlist"class="btnv6_blue_hoverfade btn_medium queue_btn_active"data-tooltip-text="该产品已在您的愿望单中。点击查看您的愿望单。"><span><img src="https://steamstore-a.akamaihd.net/public/images/v6/ico/ico_selected.png"border="0">已在愿望单中</span></a></div>`;
					}
					temp += `<div id="add_to_wishlist_area_fail"style="display: none;"><b>哎呀，很抱歉！</b></div>`;
					wish_list_innerHTML += temp;
				}
				//follow
				wish_list_innerHTML += ` <div class="queue_control_button queue_btn_follow"><div class="btnv6_blue_hoverfade btn_medium queue_btn_inactive"style=""data-tooltip-text="此功能暂时无法使用。"><span>关注</span></div><div class="btnv6_blue_hoverfade btn_medium queue_btn_active"style="display: none;"><span><img src="https://steamstore-a.akamaihd.net/public/images/v6/ico/ico_selected.png"border="0">正在关注</span></div></div>`;
				//ignore
				wish_list_innerHTML += ` <div class="queue_control_button queue_btn_ignore"><div class="btnv6_blue_hoverfade  btn_medium queue_btn_inactive"style="${(currentAppID in GDynamicStore.s_rgIgnoredApps) ? "display:none;" : ""}"data-tooltip-text="此功能暂时无法使用。"><span>忽略</span></div><div class="btnv6_blue_hoverfade  btn_medium queue_btn_active"style="${(currentAppID in GDynamicStore.s_rgIgnoredApps) ? "" : "display:none;"}"data-tooltip-text="此功能暂时无法使用。"><span><img src="https://steamstore-a.akamaihd.net/public/images/v6/ico/ico_selected.png"border="0">已忽略</span></div></div>`;
				virtualDom.querySelector(".queue_actions_ctn").innerHTML = wish_list_innerHTML;

				let temp = virtualDom.querySelector(".block.responsive_apppage_details_right.recommendation_noinfo");
				temp.parentNode.removeChild(temp);
			}
			//restart script
			virtualDom.querySelector("body").innerHTML += `<script>InitQueueControls(${currentAppID},${currentAppID},0,"1_7_7_230_12")</script>`;
			break;
		case "bundle":
		case "sub":
			break;
		case "search":
			let td = '<script>let g_cc="' + region + '";ExecuteSearch=function(rgParameters){if(g_bUseHistoryAPI){let t={...rgParameters};delete t["snr"];delete t["hide_filtered_results_warning"];if(t["sort_by"]=="_ASC")delete t["sort_by"];if(t["page"]=="1")delete t["page"];parent.history.pushState({params:t},"","?"+Object.toQueryString(t))}else{parent.window.location="#"+Object.toQueryString(rgParameters)}if(g_ajaxInFlight){g_rgDesiredParameters=rgParameters;return}if(g_rgCurrentParameters&&Object.toQueryString(g_rgCurrentParameters)==Object.toQueryString(rgParameters))return;g_rgCurrentParameters=rgParameters;let params="";for(let i in rgParameters){if(i=="cc")continue;params+=`${i}=${rgParameters[i]}&`}let url=`https://steampowered.com/search/results?${params}&cc=${g_cc}`;g_ajaxInFlight=GM_xmlhttpRequest({method:"GET",url:url,timeout:5000,headers:{Cookie:"wants_mature_content=1;mature_content=1;lastagecheckage=1-January-1970;birthtime=0"},onload:replace,ontimeout:timeout,onerror:fail});function replace(res){$J("#search_results").html(res.response);SearchCompleted(rgParameters)}function timeout(){ShowConfirmDialog("访问超时","数据获取失败...","再试一次","取消").done(result=>{result&&ExecuteSearch(rgParameters)})}function fail(){ShowConfirmDialog("访问失败","数据获取失败...","再试一次","取消").done(result=>{result&&ExecuteSearch(rgParameters)})}}</script>';
			virtualDom.querySelector("body").innerHTML += td;
			break;									
	}
}

function controlBar(){
	let postion = document.getElementById("global_action_menu");
	let menu = document.createElement("div");
	let style = document.createElement("style");
	style.innerHTML = "#cr-control-lable{position:relative;background-color:#171a21;color:#b8b6b4;width:150px;border:2px solid gray;border-radius:5px;}#cr-control-lable #select-box{margin:15px 0;padding:10px 0 5px 0;box-sizing:border-box;border:1px solid #171a21;border-radius:5px;min-width:100px;width:auto;display:none;position:absolute;background-color:#171a21}#cr-control-lable ul{max-height:300px;overflow:auto;margin:0;padding:0;}#cr-control-lable ul::-webkit-scrollbar{width:10px}#cr-control-lable ul::-webkit-scrollbar-thumb{background-color:#b8b6b4;border-radius:5px;}#cr-control-lable li{margin:0 15px;padding:5px 10px;list-style:none;border-bottom:1px solid #b8b6b4;}#cr-control-lable #select-box .modify{text-align:center;margin:5px 5px;border-radius:5px;}#cr-control-lable #select-box button{text-align:center;padding:0 5px;margin:0 2px;}#cr-control-lable .icon{position:absolute;width:7.5px;height:7.5px;box-sizing:border-box;border:7.5px solid;top:5px;left:15px;z-index:1;}#cr-control-lable .icon.on{border-color:#fff transparent transparent;}#cr-control-lable .icon.close{border-color:transparent transparent transparent #b8b6b4;}#cr-control-lable #cr-input{width:100%;text-align:center;padding-left:20px;color:#b8b6b4;background-color:#171a21;box-sizing:border-box;border-color:transparent;}#cr-control-lable #cr-input::placeholder{color:#b8b6b4;}#cr-control-lable #cr-input:focus::placeholder{color:#fff;}#cr-control-lable #cr-input:focus{outline:none;color:#fff;}#cr-control-lable button{outline:none;color:#b8b6b4;background-color:#171a21;border:none;text-align:center;box-sizing:border-box;padding:5 auto;}#cr-control-lable button:hover:active{color:#b8b6b4;}#cr-control-lable button:hover{color:#fff;cursor:pointer;}#cr-control-lable li:hover{color:#fff;cursor:pointer;}";
	// menu.style.display = "inline-block"
	document.getElementsByTagName("head")[0].appendChild(style);
	menu.innerHTML = `<div id="cr-control-lable"><div class="infobox"><div style="position: relative;"><span id="cr-icon"class="icon close"></span><input id="cr-input"type="text"name=""placeholder="输入/选择国家"autocomplete="off"></div><div style="text-align: center;"><button id="cr-check"style="width:32%;">访问</button><button id="cr-default"style="width:64%;">设置默认</button></div><div id="select-box"><ul id="cr-select-country"></ul><div class="modify"><button id="cr-modify">修改</button><button id="cr-reset">重置</button></div></div></div></div>`;
	postion.appendChild(menu);

	let input = document.getElementById("cr-input");
	let ul = document.getElementById("cr-select-country");
	let check = document.getElementById("cr-check");
	let setDefault = document.getElementById("cr-default");
	let modify = document.getElementById("cr-modify");
	let reset = document.getElementById("cr-reset");

	input.value = conf.defaultCountry;
	loadCountry();

	function loadCountry(){
		ul.innerHTML = "";
		for(let i in conf.presetCountriesList){
			ul.innerHTML += `<li>${i}</li>`;
		}
	}


	ul.addEventListener("mousedown",function(e){
		if(e.target.tagName == "LI"){
			input.value = e.target.innerHTML
		}
		e.preventDefault();
	},true);

	input.onfocus = function(){
		document.getElementById("cr-icon").className = "icon on";
		document.getElementById("select-box").style.display = "block";
	};

	input.onblur = function(){
		document.getElementById("cr-icon").className = "icon close";
		document.getElementById("select-box").style.display = "none";
	};

	input.onkeypress = function(e){
		if (e.keyCode == 13 || e.which == 13){
			check.onclick();
		}
	};

	check.onclick = function(){
		let region = input.value == "" ? conf.defaultCountry : input.value;
		blockingWaitDialog = ShowBlockingWaitDialog("正在跨区访问", `${region} 区域匿名数据获取中...`);
		anonymousAccess(region);
	};

	setDefault.onclick = function(){
		conf.defaultCountry = input.value == "" ? conf.defaultCountry : input.value;
		localStorage.setItem("STEAMCC_CONF", JSON.stringify(conf));
	};

	modify.addEventListener("mousedown",function(e){
		ShowEditablePrompt("请按照 JSON 格式修改编辑",conf,trySave,console.log)
		function trySave(res){
			try{
				res = JSON.parse(res);
				if( (typeof res.autoCheck == "boolean") && (typeof res.advancedSearch == "boolean") && (typeof res.defaultCountry == "string") && (typeof res.presetCountriesList == "object")){
					conf = res;
					localStorage.setItem("STEAMCC_CONF", JSON.stringify(conf));
					loadCountry();
				}else{
					ShowConfirmDialog("请勿修改JSON结构","<p>└──&nbspRoot</p><p>&nbsp&nbsp&nbsp&nbsp├──&nbspautoCheck&nbsp(Boolean)</p><p>&nbsp&nbsp&nbsp&nbsp├──&nbspadvancedSearch&nbsp(Boolean)</p><p>&nbsp&nbsp&nbsp&nbsp├──&nbspdefaultCountry&nbsp(String)</p><p>&nbsp&nbsp&nbsp&nbsp├──&nbsppresetCountriesList&nbsp(Object)</p>","重新编辑","取消").done( result => {
						result && ShowEditablePrompt("请按照 JSON 格式修改编辑",conf,trySave);
					});
					return;
				}
			}catch(err){
				ShowConfirmDialog("JSON 解析出错","请按照 JSON 格式修改配置","重新编辑","取消").done( result => {
						result && ShowEditablePrompt("请按照 JSON 格式修改编辑",conf,trySave);
					});
			}
		}
	},true);

	reset.addEventListener("mousedown",function(e){
		localStorage.removeItem("STEAMCC_CONF");
		ShowConfirmDialog("已重置","刷新后生效","刷新","稍后").done( result => {
			result && location.reload();
		});
	},true);
}


(function main(){
	init();
	autoCheck();
	window.onload = ()=>{
		controlBar();
	};
})();

})()