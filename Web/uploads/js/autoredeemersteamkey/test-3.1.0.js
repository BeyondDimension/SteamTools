// ==UserScript==
// @name        AuTo Redeem Steamkey
// @namespace   HCLonely
// @author      HCLonely
// @description 复制网页中的Steamkey后自动激活，3.0+版本为Beta版
// @version     Test-3.1.0
// @supportURL  https://blog.hclonely.com/posts/71381355/
// @homepage    https://blog.hclonely.com/posts/71381355/
// @iconURL     https://blog.hclonely.com/img/avatar.jpg
// @include     *://*/*
// @exclude     *store.steampowered.com/widget/*
// @exclude     *googleads*
// @grant       GM_setClipboard
// @grant       GM_addStyle
// @grant       GM_registerMenuCommand
// @grant       GM_setValue
// @grant       GM_getValue
// @grant       GM_xmlhttpRequest
// @run-at      document-idle
// @require     https://cdn.jsdelivr.net/npm/jquery@3.5.1/dist/jquery.min.js
// @require     https://cdn.jsdelivr.net/gh/HCLonely/user.js@latest/ARS/Auto_Redeem_Steamksy_static.min.js
// @require     https://cdn.jsdelivr.net/npm/sweetalert@2.1.2/dist/sweetalert.min.js
// @connect     *
// @compatible  chrome 没有测试其他浏览器的兼容性
// ==/UserScript==

/* global g_sessionID, swal, arsStatic */

(function ($jQuery) {
  'use strict'

  const url = window.location.href
  const defaultSetting = {
    newTab: false,
    copyListen: true,
    selectListen: true,
    clickListen: true,
    allKeyListen: false,
    asf: false,
    asfProtocol: 'http',
    asfHost: '127.0.0.1',
    asfPort: 1242,
    asfPassword: '',
    asfBot: ''
  }
  let sessionID = ''
  try {
    sessionID = g_sessionID // eslint-disable-line camelcase
  } catch (e) {
    sessionID = ''
  }
  if (Object.prototype.toString.call(GM_getValue('setting')) !== '[object Object]') GM_setValue('setting', defaultSetting)

  const asfCommands = $jQuery(arsStatic.html)[0]

  // 激活页面自动激活
  const selecter = url.includes('/account/registerkey') ? '' : '.hclonely '
  const autoDivideNum = 9
  const waitingSeconds = 20
  const ajaxTimeout = 15

  let keyCount = 0
  let recvCount = 0

  let allUnusedKeys = []

  const failureDetail = {
    14: '无效激活码',
    15: '重复激活',
    53: '次数上限',
    13: '地区限制',
    9: '已拥有',
    24: '缺少主游戏',
    36: '需要PS3?',
    50: '这是充值码'
  }

  const myTexts = {
    fail: '失败',
    success: '成功',
    network: '网络错误或超时',
    line: '——',
    nothing: '',
    others: '其他错误',
    unknown: '未知错误',
    redeeming: '激活中',
    waiting: '等待中',
    showUnusedKey: '显示未使用的Key',
    hideUnusedKey: '隐藏未使用的Key'
  }

  const unusedKeyReasons = [
    '次数上限',
    '地区限制',
    '已拥有',
    '缺少主游戏',
    '其他错误',
    '未知错误',
    '网络错误或超时'
  ]

  try {
    if (GM_getValue('setting').selectListen) {
      // 选中激活功能
      const icon = $jQuery(`<div class='icon-div' title='激活'><img src="${arsStatic.icon}" href="javascript:void(0)" class="icon-img"></div>`)[0]
      document.documentElement.appendChild(icon)
      document.addEventListener('mousedown', function (e) {
        if (e.target === icon || (e.target.parentNode && e.target.parentNode === icon) || (e.target.parentNode.parentNode && e.target.parentNode.parentNode === icon)) { // 点击了激活图标
          e.preventDefault()
        }
      })
      // 选中变化事件：当点击已经选中的文本的时候，隐藏激活图标和激活面板（此时浏览器动作是：选中的文本已经取消选中了）
      document.addEventListener('selectionchange', () => {
        if (!window.getSelection().toString().trim()) {
          icon.style.display = 'none'
        }
      })
      // 鼠标事件：防止选中的文本消失；显示、隐藏激活图标
      document.addEventListener('mouseup', function (e) {
        if (e.target === icon || (e.target.parentNode && e.target.parentNode === icon) || (e.target.parentNode.parentNode && e.target.parentNode.parentNode === icon)) { // 点击了激活图标
          e.preventDefault()
          return
        }
        const text = window.getSelection().toString().trim()
        const productKey = window.getSelection().toString().trim() || e.target.value
        if (/[\d\w]{5}(-[\d\w]{5}){2}/.test(productKey) && text && icon.style.display === 'none') {
          icon.style.top = e.pageY + 12 + 'px'
          icon.style.left = e.pageX + 18 + 'px'
          icon.style.display = 'block'
        } else if (!text) {
          icon.style.display = 'none'
        }
      })
      // 激活图标点击事件
      icon.addEventListener('click', function (e) {
        const productKey = window.getSelection().toString().trim() || e.target.value
        registerkey(productKey)
      })
    }

    // 复制激活功能
    if (!/https?:\/\/store\.steampowered\.com\/account\/registerkey[\w\W]{0,}/.test(url) && GM_getValue('setting').copyListen) { // 非激活页面
      const activateProduct = function (e) {
        const productKey = window.getSelection().toString().trim() || e.target.value
        if (/^([\w\W]*)?([\d\w]{5}(-[\d\w]{5}){2}(\r||,||，)?){1,}/.test(productKey)) {
          if (!$jQuery('div.swal-overlay').hasClass('swal-overlay--show-modal')) {
            swal({
              title: '检测到神秘key,是否激活？',
              icon: 'success',
              buttons: {
                confirm: '激活',
                cancel: '取消'
              }
            }).then((value) => {
              if (value) registerkey(productKey)
            })
          }
        } else if (/^!addlicense.*[\d]+$/gi.test(productKey)) {
          if (Object.prototype.toString.call(GM_getValue('setting')) === '[object Object]' && GM_getValue('setting').asf && !$jQuery('div.swal-overlay').hasClass('swal-overlay--show-modal')) {
            swal({
              closeOnClickOutside: false,
              className: 'swal-user',
              title: '检测到您复制了以下ASF指令，是否执行？',
              text: productKey,
              buttons: {
                confirm: '执行',
                cancel: '取消'
              }
            }).then((value) => {
              if (value) asfRedeem(productKey)
            })
          }
        }
      }
      window.addEventListener('copy', activateProduct, false)
    }

    if (/^https?:\/\/store\.steampowered\.com\/account\/registerkey*/.test(url)) {
      $jQuery('#registerkey_examples_text').html(
        '<div class="notice_box_content" id="unusedKeyArea" style="display: none">' +
                    '<b>未使用的Key：</b><a tabindex="300" class="btnv6_blue_hoverfade btn_medium" id="copyUnuseKey"><span>提取未使用key</span></a><br>' +
                    '<div><ol id="unusedKeys">' +
                    '</ol></div>' +
                    '</div>' +

                    '<div class="table-responsive table-condensed">' +
                    '<table class="table table-hover" style="display: none">' +
                    '<caption><h2>激活记录</h2></caption><thead><th>No.</th><th>Key</th>' +
                    '<th>结果</th><th>详情</th><th>Sub</th></thead><tbody></tbody>' +
                    '</table></div><br>')

      $jQuery('#copyUnuseKey').click(() => {
        GM_setClipboard(arr(getKeysByRE($jQuery('#unusedKeys').text())).join(','))
        swal({ title: '复制成功！', icon: 'success' })
      })
      $jQuery('.registerkey_input_box_text').parent().css('float', 'none')
      $jQuery('.registerkey_input_box_text').parent().append('<textarea class="form-control" rows="3"' +
          ' id="inputKey" placeholder="支持批量激活，可以把整个网页文字复制过来&#10;' +
          '若一次激活的Key的数量超过9个则会自动分批激活（等待20秒）&#10;' +
          '激活多个SUB时每个SUB之间用英文逗号隔开&#10;' +
          ' style="margin: 3px 0px 0px; width: 525px; height: 102px;"></textarea><br>');
      /^https?:\/\/store\.steampowered\.com\/account\/registerkey\?key=[\w\W]+/.test(url) && (document.getElementById('inputKey').value = url.replace(/https?:\/\/store\.steampowered\.com\/account\/registerkey\?key=/i, ''))
      $jQuery('.registerkey_input_box_text').hide()
      $jQuery('#purchase_confirm_ssa').hide()

      $jQuery('#register_btn').parent().css('margin', '10px 0')
      $jQuery('#register_btn').parent().append('<a tabindex="300" class="btnv6_blue_hoverfade btn_medium" style="margin-left:0"' +
          ' id="redeemKey"><span>激活key</span></a>' + ' &nbsp;&nbsp;' +
          '<a tabindex="300" class="btnv6_blue_hoverfade btn_medium" style="margin-left:0"' +
          ' id="redeemSub"><span>激活sub</span></a>' + ' &nbsp;&nbsp;' +
          '<a tabindex="300" class="btnv6_blue_hoverfade btn_medium" style="margin-left:0"' +
          ' id="changeCountry"><span>更换国家/地区</span></a>' + ' &nbsp;&nbsp;')
      $jQuery('#register_btn').remove();
      /^https?:\/\/store\.steampowered\.com\/account\/registerkey\?key=[\w\W]+/.test(url) && (redeem(getKeysByRE(url.replace(/https?:\/\/store\.steampowered\.com\/account\/registerkey\?key=/i, '').trim())))
      $jQuery('#redeemKey').click(() => { redeemKeys() })
      $jQuery('#redeemSub').click(redeemSubs)
      $jQuery('#changeCountry').click(cc)

      toggleUnusedKeyArea()
    } else if (/https?:\/\/steamdb\.info\/freepackages\//.test(url)) { // steamdb.info点击自动跳转到激活页面
      const activateConsole = function (e) {
        const sub = []
        $('#freepackages span:visible').map(function () {
          sub.push($(this).attr('data-subid'))
        })
        const freePackages = sub.join(',')
        // const setting = GM_getValue('setting')
        window.open('https://store.steampowered.com/account/licenses/?sub=' + freePackages, '_self')
        // if(setting.asf) asfRedeem("!addlicense "+(setting.asfBot||"asf")+" "+freePackages);
        // else window.open("https://store.steampowered.com/account/licenses/?sub=" + freePackages, "_self");
      }
      const fp = setInterval(() => {
        if (document.getElementById('freepackages')) {
          document.getElementById('freepackages').onclick = activateConsole
          clearInterval(fp)
        }
      }, 1000)
    } else if (/https?:\/\/store\.steampowered\.com\/account\/licenses\/(\?sub=[\w\W]{0,})?/.test(url)) { // 自动添加sub
      $jQuery('.pageheader').parent().append('<div style="float: left;";>' +
          '<textarea class="registerkey_input_box_text" rows="1"' + 'name="product_key"' +
          ' id="gameSub" placeholder="输入SUB,多个SUB之间用英文逗号连接"' + 'value=""' + 'color:#fff;' +
          ' style="margin: 3px 0px 0px; width: 400px; height: 15px;background-color:#102634; padding: 6px 18px 6px 18px; font-weight:bold; color:#fff;"></textarea>' +
          ' &nbsp ' + '</div>' + '<a tabindex="300" class="btnv6_blue_hoverfade btn_medium"' +
          ' style="width: 95px; height: 30px;"' +
          ' id="buttonSUB"><span>激活SUB</span></a>' + '<a tabindex="300" class="btnv6_blue_hoverfade btn_medium"' +
          ' style="width: 125px; height: 30px;margin-left:5px"' +
          ' id="changeCountry"><span>更改国家/地区</span></a>')
      $jQuery('#buttonSUB').click(() => { redeemSub() })
      $jQuery('#changeCountry').click(cc)
      if (/https?:\/\/store\.steampowered\.com\/account\/licenses\/\?sub=([\d]{1,},){1,}/.test(url)) {
        setTimeout(() => { redeemSub(url) }, 2000)
      }
    } else if (GM_getValue('setting').clickListen) { // 点击添加链接
      let htmlEl
      if (window.document.body) {
        window.document.body.onclick = function (event) {
          htmlEl = event.target// 鼠标每经过一个元素，就把该元素赋值给变量htmlEl
          if ($jQuery(htmlEl).parents('.swal-overlay').length === 0 && htmlEl.tagName !== 'A' && htmlEl.tagName !== 'BUTTON' && htmlEl.getAttribute('type') !== 'button' && htmlEl.tagName !== 'TEXTAREA' && htmlEl.getAttribute('type') !== 'text') {
            if (($jQuery(htmlEl).children().length === 0 || !/([0-9,A-Z]{5}-){2,4}[0-9,A-Z]{5}/gim.test($jQuery.makeArray($jQuery(htmlEl).children().map(function () {
              return $jQuery(this).text()
            })).join(''))) && /([0-9,A-Z]{5}-){2,4}[0-9,A-Z]{5}/gim.test($jQuery(htmlEl).text())) {
              mouseClick($jQuery, event)
              arr($jQuery(htmlEl).text().match(/[\w\d]{5}(-[\w\d]{5}){2}/gim)).map(function (e) {
                $jQuery(htmlEl).html($jQuery(htmlEl).html().replace(new RegExp(e, 'gi'), `<a class="redee-key" href='javascript:void(0)' target="_self" key='${e}'>${e}</a>`))
              })
              $jQuery('.redee-key').click(function () {
                registerkey($jQuery(this).attr('key'), 1)
              })
            }
          }
        }
      }
    }
    if (GM_getValue('setting').allKeyListen) { // 激活页面内所有key
      redeemAllKey()
    }

    GM_addStyle(arsStatic.css)

    GM_registerMenuCommand('⚙设置', setting)
    GM_registerMenuCommand('执行ASF指令', asfSend)
    GM_registerMenuCommand('查看上次激活记录', showHistory)
    GM_registerMenuCommand('Key格式转换', showSwitchKey)
    GM_registerMenuCommand('新版使用说明', () => { window.open('https://keylol.com/t344489-1-1', '_blank') })
  } catch (e) {
    swal('AuTo Redeem Steamkey脚本执行出错，详情请查看控制台！', e.stack, 'error')
    console.error(e)
  }
  function redeemKey (key) {
    GM_xmlhttpRequest({
      url: 'https://store.steampowered.com/account/ajaxregisterkey/',
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
        Origin: 'https://store.steampowered.com',
        Referer: 'https://store.steampowered.com/account/registerkey'
      },
      data: `product_key=${key}&sessionid=${sessionID}`,
      method: 'POST',
      responseType: 'json',
      timeout: 1000 * ajaxTimeout,
      onloadstart: function () {
        if (jQuery(selecter + 'table').is(':hidden')) {
          jQuery(selecter + 'table').fadeIn()
        }
      },
      onload: function (response) {
        if (response.status === 200 && response.response) {
          const data = response.response
          if (data.success === 1) {
            tableUpdateKey(key, myTexts.success, myTexts.line,
              data.purchase_receipt_info.line_items[0].packageid,
              data.purchase_receipt_info.line_items[0].line_item_description)
            return
          } else if (data.purchase_result_details !== undefined && data.purchase_receipt_info) {
            if (!data.purchase_receipt_info.line_items[0]) {
              tableUpdateKey(key, myTexts.fail,
                failureDetail[data.purchase_result_details] ? failureDetail[data.purchase_result_details] : myTexts.others,
                0, myTexts.nothing)
            } else {
              tableUpdateKey(key, myTexts.fail,
                failureDetail[data.purchase_result_details] ? failureDetail[data.purchase_result_details] : myTexts.others,
                data.purchase_receipt_info.line_items[0].packageid,
                data.purchase_receipt_info.line_items[0].line_item_description)
            }
            return
          }
          tableUpdateKey(key, myTexts.fail, myTexts.nothing, 0, myTexts.nothing)
        } else {
          tableUpdateKey(key, myTexts.fail, myTexts.network, 0, myTexts.nothing)
        }
      }
    })
  }

  function setUnusedKeys (key, success, reason, subId, subName) {
    if (success && allUnusedKeys.includes(key)) {
      var listObject
      allUnusedKeys = allUnusedKeys.filter(function (keyItem) {
        return keyItem !== key
      })

      $jQuery(selecter + 'li').map((i, e) => {
        if ($jQuery(e).html().includes(key)) {
          listObject.remove()
        }
      })
    } else if (!success && !allUnusedKeys.includes(key) && unusedKeyReasons.includes(reason)) {
      listObject = $jQuery('<li></li>')
      listObject.html(key + ' ( ' + reason +
        (subId !== 0 ? (': <code>' + subId + '</code> ' + subName) : '') +
        ' )')
      $jQuery('#unusedKeys').append(listObject)

      allUnusedKeys.push(key)
    }
  }

  function tableInsertKey (key) {
    keyCount++
    const row = $jQuery('<tr></tr>')
    row.append('<td class="nobr">' + keyCount + '</td>')
    row.append('<td class="nobr"><code>' + key + '</code></td>')
    row.append('<td colspan="3">' + myTexts.redeeming + '...</td>')

    $jQuery(selecter + 'tbody').prepend(row)
  }

  function tableWaitKey (key) {
    keyCount++
    const row = $jQuery('<tr></tr>')
    row.append('<td class="nobr">' + keyCount + '</td>')
    row.append('<td class="nobr"><code>' + key + '</code></td>')
    row.append('<td colspan="3">' + myTexts.waiting + ' (' + waitingSeconds + '秒)...</td>')

    $jQuery(selecter + 'tbody').prepend(row)
  }

  function tableUpdateKey (key, result, detail, subId, subName) {
    setUnusedKeys(key, result === myTexts.success, detail, subId, subName)

    recvCount++
    if (!selecter && recvCount === keyCount) {
      $jQuery('#buttonRedeem').fadeIn()
      $jQuery('#inputKey').removeAttr('disabled')
    }

    var rowObjects = $jQuery(selecter + 'tr')
    for (let i = 1; i < rowObjects.length; i++) {
      const rowElement = rowObjects[i]
      const rowObject = $jQuery(rowElement)

      if (rowObject.children()[1].innerHTML.includes(key) && rowObject.children()[2].innerHTML.includes(myTexts.redeeming)) {
        rowObject.children()[2].remove()
        if (result === myTexts.fail) rowObject.append('<td class="nobr" style="color:red">' + result + '</td>')
        else rowObject.append('<td class="nobr" style="color:green">' + result + '</td>')
        rowObject.append('<td class="nobr">' + detail + '</td>')
        if (subId === 0) {
          rowObject.append('<td>——</td>')
        } else {
          rowObject.append('<td><code>' + subId + '</code> <a href="https://steamdb.info/sub/' +
            subId + '/" target="_blank">' + subName + '</a></td>')
        }
        break
      }
    }
  }

  function startTimer () {
    const timer = setInterval(function () {
      let flag = false
      let nowKey = 0

      const rowObjects = $jQuery(selecter + 'tr')
      for (let i = rowObjects.length - 1; i >= 1; i--) {
        const rowElement = rowObjects[i]
        const rowObject = $jQuery(rowElement)
        if (rowObject.children()[2].innerHTML.includes(myTexts.waiting)) {
          nowKey++
          if (nowKey <= autoDivideNum) {
            let key = rowObject.children()[1].innerHTML.substring(6)
            key = key.substring(0, key.indexOf('</code>'))
            rowObject.children()[2].innerHTML = '<td colspan="3">' + myTexts.redeeming + '...</td>'
            redeemKey(key)
          } else {
            flag = true
            break
          }
        }
      }
      if (!flag) {
        clearInterval(timer)
      }
    }, 1000 * waitingSeconds)
  }

  function redeem (keys) {
    if (keys.length <= 0) {
      return
    }

    if (!selecter) {
      $jQuery('#buttonRedeem').hide()
      $jQuery('#inputKey').attr('disabled', 'disabled')
    }

    let nowKey = 0
    keys.forEach(function (key) {
      nowKey++
      if (nowKey <= autoDivideNum) {
        tableInsertKey(key)
        redeemKey(key)
      } else {
        tableWaitKey(key)
      }
    })

    if (nowKey > autoDivideNum) {
      startTimer()
    }
  }
  function redeemKeys (key) {
    const keys = key || getKeysByRE($jQuery('#inputKey').val().trim())
    redeem(keys)
  }

  function toggleUnusedKeyArea () {
    if (!selecter) {
      if ($jQuery('#unusedKeyArea').is(':hidden')) {
        $jQuery('#unusedKeyArea').show()
      } else {
        $jQuery('#unusedKeyArea').hide()
      }
    }
  }

  function setting () {
    const setting = Object.prototype.toString.call(GM_getValue('setting')) === '[object Object]' ? GM_getValue('setting') : defaultSetting
    const div = $jQuery(`
<div id="hclonely-asf">
<input type="checkbox" name="newTab" ${setting.newTab ? 'checked=checked' : ''} title="开启ASF激活后此功能无效"/><span title="开启ASF激活后此功能无效">新标签页激活</span><br/>
<input type="checkbox" name="copyListen" ${setting.copyListen ? 'checked=checked' : ''} title="复制key时询问是否激活"/><span title="复制key时询问是否激活">开启复制捕捉</span>
<input type="checkbox" name="selectListen" ${setting.selectListen ? 'checked=checked' : ''} title="选中key时显示激活图标"/><span title="选中key时显示激活图标">开启选中捕捉</span>
<input type="checkbox" name="clickListen" ${setting.clickListen ? 'checked=checked' : ''} title="点击key时添加激活链接"/><span title="点击key时添加激活链接">开启点击捕捉</span><br/>
<input type="checkbox" name="allKeyListen" ${setting.allKeyListen ? 'checked=checked' : ''} title="匹配页面内所有符合steam key格式的内容"/><span title="匹配页面内所有符合steam key格式的内容">捕捉页面内所有key</span>
<div class="swal-title">ASF IPC设置</div>
<span>ASF IPC协议</span><input type="text" name="asfProtocol" value='${setting.asfProtocol}' placeholder="http或https,默认为http"/><br/>
<span>ASF IPC地址</span><input type="text" name="asfHost" value='${setting.asfHost}' placeholder="ip地址或域名,默认为127.0.0.1"/><br/>
<span>ASF IPC端口</span><input type="text" name="asfPort" value='${setting.asfPort}' placeholder="默认1242"/><br/>
<span>ASF IPC密码</span><input type="text" name="asfPassword" value='${setting.asfPassword}' placeholder="ASF IPC密码"/><br/>
<span>ASF Bot名字</span><input type="text" name="asfBot" value='${setting.asfBot}' placeholder="ASF Bot name,可留空"/><br/>
<input type="checkbox" name="asf" ${setting.asf ? 'checked=checked' : ''} title="此功能默认关闭新标签页激活"/><span title="此功能默认关闭新标签页激活">开启ASF激活</span>
</div>`)[0]

    swal({
      closeOnClickOutside: false,
      className: 'asf-class',
      title: '全局设置',
      content: div,
      buttons: {
        confirm: '保存',
        cancel: '取消'
      }
    })
      .then((value) => {
        if (value) {
          const setting = {}
          $jQuery('#hclonely-asf input').map(function () {
            setting[$jQuery(this).attr('name')] = this.value === 'on' ? this.checked : this.value
          })
          GM_setValue('setting', setting)
          swal({
            closeOnClickOutside: false,
            icon: 'success',
            title: '保存成功！',
            text: '刷新页面后生效！',
            buttons: {
              confirm: '确定'
            }
          })
        }
      })
  }
  function asfSend (c = '') {
    if (Object.prototype.toString.call(GM_getValue('setting')) === '[object Object]' && GM_getValue('setting').asf) {
      swal({
        closeOnClickOutside: false,
        className: 'swal-user',
        text: '请在下方输入要执行的ASF指令：',
        content: 'input',
        buttons: {
          test: '连接测试',
          redeem: '激活key',
          pause: '暂停挂卡',
          resume: '恢复挂卡',
          '2fa': '获取令牌',
          more: '更多ASF指令',
          confirm: '确定',
          cancel: '取消'
        }
      }).then((value) => {
        switch (value) {
          case 'redeem':
            swalRedeem()
            break
          case 'pause':
          case 'resume':
          case '2fa':
            asfRedeem('!' + value)
            break
          case 'test':
            asfTest()
            break
          case 'more':
            swal({
              closeOnClickOutside: false,
              className: 'swal-user',
              text: 'ASF指令',
              content: asfCommands,
              buttons: {
                confirm: '返回',
                cancel: '关闭'
              }
            }).then((value) => {
              if (value) asfSend()
            })
            $jQuery('table.hclonely button.swal-button').click(function () {
              const setting = Object.prototype.toString.call(GM_getValue('setting')) === '[object Object]' ? GM_getValue('setting') : defaultSetting
              const command = setting.asfBot ? $jQuery(this).parent().next().text().trim().replace(/<Bots>/gim, setting.asfBot) : $jQuery(this).parent().next().text().trim()
              asfSend(command)
            })
            break
          case null:
            break
          default:
            if (!$jQuery('.swal-content__input').val()) {
              swal({
                closeOnClickOutside: false,
                title: 'ASF指令不能为空！',
                icon: 'warning',
                buttons: {
                  confirm: '确定'
                }
              }).then(() => { asfSend(c) })
            } else {
              const v = value || $jQuery('.swal-content__input').val()
              if (v) asfRedeem(v)
            }
            break
        }
      })
      if (c) $jQuery('.swal-content__input').val('!' + c)
    } else {
      swal({
        closeOnClickOutside: false,
        className: 'swal-user',
        icon: 'warning',
        title: '此功能需要在设置中配置ASF IPC并开启ASF功能！',
        buttons: {
          confirm: '确定'
        }
      })
    }
  }

  function swalRedeem () {
    swal({
      closeOnClickOutside: false,
      className: 'swal-user',
      title: '请输入要激活的key:',
      content: $jQuery('<textarea id=\'keyText\' class=\'asf-output\'></textarea>')[0],
      buttons: {
        confirm: '激活',
        cancel: '返回'
      }
    }).then((value) => {
      if (value) {
        const key = getKeysByRE($jQuery('#keyText').val())
        if (key.length > 0) asfRedeem('!redeem ' + (GM_getValue('setting').asfBot ? (GM_getValue('setting').asfBot + ' ') : '') + key.join(','))
        else {
          swal({
            closeOnClickOutside: false,
            title: 'steam key不能为空！',
            icon: 'error',
            buttons: {
              confirm: '返回',
              cancel: '关闭'
            }
          }).then((value) => {
            if (value) swalRedeem()
          })
        }
      } else {
        asfSend()
      }
    })
  }
  function asfTest () {
    const setting = GM_getValue('setting') || {}
    if (setting.asf) {
      swal({
        closeOnClickOutside: false,
        title: 'ASF连接测试',
        text: '正在尝试连接 "' + setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/Api/Command/"',
        buttons: {
          confirm: '确定'
        }
      })
      GM_xmlhttpRequest({
        method: 'POST',
        url: setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/Api/Command/',
        data: '{"Command":"!stats"}',
        responseType: 'json',
        headers: {
          accept: 'application/json',
          'Content-Type': 'application/json',
          Authentication: setting.asfPassword,
          Host: setting.asfHost + ':' + setting.asfPort,
          Origin: setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort,
          Referer: setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/page/commands'
        },
        onload: function (data) {
          if (data.status === 200) {
            if (data.response.Success === true && data.response.Message === 'OK' && data.response.Result) {
              swal({
                closeOnClickOutside: false,
                title: 'ASF连接成功！',
                icon: 'success',
                text: '连接地址 "' + setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/Api/Command/" \n返回内容 "' + data.response.Result.trim() + '"',
                buttons: {
                  confirm: '确定'
                }
              })
            } else if (data.response.Message) {
              swal({
                closeOnClickOutside: false,
                title: 'ASF连接成功？',
                icon: 'info',
                text: '连接地址 "' + setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/Api/Command/" \n返回内容 "' + data.response.Message.trim() + '"',
                buttons: {
                  confirm: '确定'
                }
              })
            } else {
              swal({
                closeOnClickOutside: false,
                title: 'ASF连接失败！',
                icon: 'error',
                text: '连接地址 "' + setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/Api/Command/" \n返回内容 "' + data.responseText + '"',
                buttons: {
                  confirm: '确定'
                }
              })
            }
          } else {
            swal({
              closeOnClickOutside: false,
              title: 'ASF连接失败：' + data.status,
              icon: 'error',
              text: '连接地址 "' + setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/Api/Command/"',
              buttons: {
                confirm: '确定'
              }
            })
          }
        }
      })
    } else {
      swal({
        closeOnClickOutside: false,
        title: '请先在设置中开启ASF功能',
        icon: 'warning',
        buttons: {
          confirm: '确定'
        }
      })
    }
  }

  function showHistory () {
    const history = GM_getValue('history')
    if (Array.isArray(history)) {
      swal({
        closeOnClickOutside: false,
        className: 'swal-user',
        title: '上次激活记录：',
        content: $jQuery(history[0])[0],
        buttons: {
          confirm: '确定'
        }
      })
      if (history[1]) $jQuery('.swal-content textarea').val(history[1])
    } else {
      swal({
        closeOnClickOutside: false,
        title: '没有操作记录！',
        icon: 'error',
        buttons: {
          cancel: '关闭'
        }
      })
    }
  }

  function showSwitchKey () {
    swal({
      closeOnClickOutside: false,
      title: '请选择要转换成什么格式：',
      buttons: {
        confirm: '确定',
        cancel: '关闭'
      },
      content: $jQuery('<div class=\'switch-key\'><div class=\'switch-key-left\'><p>key</p><p>key</p><p>key</p><input name=\'keyType\' type=\'radio\' value=\'1\'/></div><div class=\'switch-key-right\'><p>&nbsp;</p><p>key,key,key</p><p>&nbsp;</p><input name=\'keyType\' type=\'radio\' value=\'2\'/></div></div>')[0]
    }).then((value) => {
      if (value) {
        if ($jQuery('input:radio:checked').val()) {
          showSwitchArea($jQuery('input:radio:checked').val())
        } else {
          swal({
            closeOnClickOutside: false,
            title: '请选择要将key转换成什么格式！',
            icon: 'warning'
          }).then(showSwitchKey)
        }
      }
    })
    function showSwitchArea (type) {
      swal({
        closeOnClickOutside: false,
        title: '请输入要转换的key:',
        content: $jQuery('<textarea style=\'width: 80%;height: 100px;\'></textarea>')[0],
        buttons: {
          confirm: '转换',
          back: '返回',
          cancel: '关闭'
        }
      }).then((value) => {
        if (value === 'back') {
          showSwitchKey(type)
        } else if (value) {
          switchKey($jQuery('.swal-content textarea').val(), type)
        }
      })
    }
    function switchKey (key, type) {
      switch (type) {
        case '1':
          showKey(getKeysByRE(key).join('\n'), type)
          break
        case '2':
          showKey(getKeysByRE(key).join(','), type)
          break
        default:
          break
      }
    }
    function showKey (key, type) {
      swal({
        closeOnClickOutside: false,
        icon: 'success',
        title: '转换成功！',
        content: $jQuery(`<textarea style='width: 80%;height: 100px;' value='${key}' readonly='readonly'>${key}</textarea>`)[0],
        buttons: {
          confirm: '返回',
          cancel: '关闭'
        }
      }).then((value) => {
        if (value) {
          showSwitchArea(type)
        }
      })
      $jQuery('.swal-content textarea').click(function () { this.select() })
    }
    $jQuery('.switch-key div').map(function () {
      $jQuery(this).click(function () {
        $jQuery(this).find('input')[0].click()
      })
    })
  }

  function getKeysByRE (text) {
    text = text.trim().toUpperCase()
    const reg = new RegExp('([0-9,A-Z]{5}-){2,4}[0-9,A-Z]{5}', 'g')
    const keys = []

    let result
    while (result = reg.exec(text)) { // eslint-disable-line no-cond-assign
      keys.push(result[0])
    }

    return keys
  }

  function registerkey (key) {
    const setting = GM_getValue('setting')
    const keys = getKeysByRE(key)
    if (setting.asf) asfRedeem('!redeem ' + (setting.asfBot ? (setting.asfBot + ' ') : '') + keys.join(','))
    else if (setting.newTab) window.open('https://store.steampowered.com/account/registerkey?key=' + keys.join(','), '_blank')
    else webRedeem(keys)
  }
  function asfRedeem (command) {
    const setting = GM_getValue('setting')
    const textarea = document.createElement('textarea')
    textarea.setAttribute('class', 'asf-output')
    textarea.setAttribute('readonly', 'readonly')
    const btn = /!redeem/gim.test(command) ? { confirm: '提取未使用key', cancel: '关闭' } : { confirm: '确定' }
    swal({
      closeOnClickOutside: false,
      className: 'swal-user',
      text: '正在执行ASF指令：' + command,
      content: textarea,
      buttons: btn
    }).then((v) => {
      if (/!redeem/gim.test(command)) {
        let value = ''
        if ($jQuery('.swal-content textarea').length > 0) {
          value = $jQuery('.swal-content textarea').val()
        }
        GM_setValue('history', [$jQuery('.swal-content').html(), value])
        if (v) {
          const unUseKey = $jQuery('.swal-content textarea').val().split(/[(\r\n)\r\n]+/).map(function (e) {
            if (/未使用/gim.test(e)) {
              return e
            }
          }).join(',')
          GM_setClipboard(arr(getKeysByRE(unUseKey)).join(','))
          swal({ title: '复制成功！', icon: 'success' })
        }
      }
    })
    GM_xmlhttpRequest({
      method: 'POST',
      url: setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/Api/Command',
      data: `{"Command":"${command}"}`,
      responseType: 'json',
      headers: {
        accept: 'application/json',
        'Content-Type': 'application/json',
        Authentication: setting.asfPassword,
        Host: setting.asfHost + ':' + setting.asfPort,
        Origin: setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort,
        Referer: setting.asfProtocol + '://' + setting.asfHost + ':' + setting.asfPort + '/page/commands'
      },
      onload: function (data) {
        console.log(data)
        console.log(command)
        if (data.status === 200) {
          if (data.response.Success === true && data.response.Message === 'OK' && data.response.Result) {
            textarea.value += data.response.Result.trim() + ' \n'
          } else if (data.response.Message) {
            textarea.value += data.response.Message.trim() + ' \n'
          } else {
            textarea.value += data.responseText
          }
        } else {
          swal({
            closeOnClickOutside: false,
            className: 'swal-user',
            title: '执行ASF指令(' + command + ')失败！请检查ASF配置是否正确！',
            text: data.text || data.status,
            icon: 'error',
            buttons: {
              confirm: '关闭'
            }
          })
        }
      }
    })
  }
  function webRedeem (key) {
    const div = $jQuery('<div id="registerkey_examples_text"><div class="notice_box_content" id="unusedKeyArea"> <b>未使用的Key：</b><br><div><ol id="unusedKeys" align="left"></ol></div></div><div class="table-responsive table-condensed"><table class="table table-hover hclonely"><caption><h2>激活记录</h2></caption><thead><th>No.</th><th>Key</th><th>结果</th><th>详情</th><th>Sub</th></thead><tbody></tbody></table></div><br></div>')[0]
    swal({
      closeOnClickOutside: false,
      className: 'swal-user',
      title: '正在获取sessionID...',
      buttons: {
        confirm: '关闭'
      }
    })
    if (sessionID) {
      swal({
        closeOnClickOutside: false,
        className: 'swal-user',
        title: '正在激活steam key...',
        content: div,
        buttons: {
          confirm: '提取未使用key',
          cancel: '关闭'
        }
      }).then((v) => {
        let value = ''
        if ($jQuery('.swal-content textarea').length > 0) {
          value = $jQuery('.swal-content textarea').val()
        }
        GM_setValue('history', [$jQuery('.swal-content').html(), value])
        if (v) {
          GM_setClipboard(arr(getKeysByRE($jQuery('#unusedKeys').text())).join(','))
          swal({ title: '复制成功！', icon: 'success' })
        }
      })
      redeemKeys(key)
    } else {
      GM_xmlhttpRequest({
        method: 'GET',
        url: 'https://store.steampowered.com/account/registerkey',
        onload: function (data) {
          if (data.finalUrl.includes('login')) {
            swal({
              closeOnClickOutside: false,
              icon: 'warning',
              title: '请先登录steam！',
              buttons: {
                confirm: '登录',
                cancel: '关闭'
              }
            }).then((value) => {
              if (value) window.open('https://store.steampowered.com/login/', '_blank')
            })
          } else {
            if (data.status === 200) {
              const gSessionId = data.responseText.match(/g_sessionID = "(.+?)";/)
              sessionID = gSessionId === null ? null : gSessionId[1]
              swal({
                closeOnClickOutside: false,
                className: 'swal-user',
                title: '正在激活steam key...',
                content: div,
                buttons: {
                  confirm: '提取未使用key',
                  cancel: '关闭'
                }
              }).then((v) => {
                let value = ''
                if ($jQuery('.swal-content textarea').length > 0) {
                  value = $jQuery('.swal-content textarea').val()
                }
                GM_setValue('history', [$jQuery('.swal-content').html(), value])
                if (v) {
                  GM_setClipboard(getKeysByRE($jQuery('#unusedKeys').text()).join(','))
                  swal({ title: '复制成功！', icon: 'success' })
                }
              })
              redeemKeys(key)
            } else {
              swal({
                closeOnClickOutside: false,
                className: 'swal-user',
                title: '获取sessionID失败！',
                icon: 'error',
                buttons: {
                  confirm: '关闭'
                }
              })
            }
          }
        }
      })
    }
  }

  function redeemSub (e) {
    const subText = e || document.getElementById('gameSub').value
    if (subText) {
      const ownedPackages = {}
      $jQuery('.account_table a').each(function (i, el) {
        const match = el.href.match(/javascript:RemoveFreeLicense\( ([0-9]+), '/)
        if (match !== null) {
          ownedPackages[+match[1]] = true
        }
      })
      const freePackages = subText.match(/[\d]{2,}/g)
      let i = 0
      let loaded = 0
      let packae = 0
      const total = freePackages.length
      swal('正在执行…', '请等待所有请求完成。 忽略所有错误，让它完成。')
      for (; i < total; i++) {
        packae = freePackages[i]
        if (ownedPackages[packae]) {
          loaded++
          continue
        }
        $jQuery.post('//store.steampowered.com/checkout/addfreelicense', {
          action: 'add_to_cart',
          sessionid: g_sessionID,
          subid: packae
        }).always(function () {
          loaded++
          if (loaded >= total) {
            if (url.includes('licenses')) {
              window.open('https://store.steampowered.com/account/licenses/', '_self')
            } else {
              swal('全部激活完成，是否前往账户页面查看结果？', {
                buttons: {
                  cancel: '取消',
                  确定: true
                }
              })
                .then((value) => {
                  if (value) window.open('https://store.steampowered.com/account/licenses/', '_blank')
                })
            }
          } else {
            swal('正在激活…', '进度：' + loaded + '/' + total + '.')
          }
        })
      }
    }
  }
  function cc () {
    swal({
      closeOnClickOutside: false,
      icon: 'info',
      title: '正在获取当前国家/地区...'
    })
    $jQuery.ajax({
      url: '//store.steampowered.com/cart/',
      type: 'get',
      success: function (data) {
        if (data.match(/id="usercountrycurrency_trigger"[\w\W]*?>[w\W]*?<\/a/gim)) {
          const c = data.match(/id="usercountrycurrency_trigger"[\w\W]*?>[w\W]*?<\/a/gim)[0].replace(/id="usercountrycurrency_trigger"[\w\W]*?>|<\/a/g, '')
          // const thisC = data.match(/id="usercountrycurrency"[\w\W]*?value=".*?"/gim)[0].match(/value=".*?"/gim)[0].replace(/value="|"/g, '')
          const div = data.match(/<div class="currency_change_options">[\w\W]*?<p/gim)[0].replace(/[\s]*?<p/gim, '') + '</div>'
          // $jQuery("body").append(`<div id="nowCountry" class="ellipsis" data-country="${thisC}" style="font-size:20px;">转换商店和钱包&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;当前国家/地区：${c}</div><div style="padding:20px">${div}</div></div>`);
          swal({
            closeOnClickOutside: false,
            title: `当前国家/地区：${c}`,
            content: $jQuery(`<div>${div}</div>`)[0]
          })
          $jQuery('.currency_change_option').click(function () {
            changeCountry($jQuery(this).attr('data-country'))
          })
        } else {
          swal('需要挂相应地区的梯子！', '', 'warning')
        }
      },
      error: () => {
        swal('获取当前国家/地区失败！', '', 'error')
      }
    })
  }
  function changeCountry (country) {
    swal({
      closeOnClickOutside: false,
      icon: 'info',
      title: '正在更换国家/地区...'
    })
    $jQuery.ajax({
      url: '//store.steampowered.com/account/setcountry',
      type: 'post',
      data: {
        sessionid: g_sessionID,
        cc: country
      },
      complete: function () {
        $jQuery.ajax({
          url: '//store.steampowered.com/cart/',
          type: 'get',
          success: function (data) {
            const c = data.match(/id="usercountrycurrency_trigger"[\w\W]*?>[w\W]*?<\/a/gim)[0].replace(/id="usercountrycurrency_trigger"[\w\W]*?>|<\/a/g, '')
            const thisC = data.match(/id="usercountrycurrency"[\w\W]*?value=".*?"/gim)[0].match(/value=".*?"/gim)[0].replace(/value="|"/g, '')
            const div = data.match(/<div class="currency_change_options">[\w\W]*?<p/gim)[0].replace(/[\s]*?<p/gim, '') + '</div>'

            if (thisC === country) {
              swal('更换成功！', '', 'success').then(() => {
                swal({
                  closeOnClickOutside: false,
                  title: `当前国家/地区：${c}`,
                  content: $jQuery(`<div>${div}</div>`)[0]
                })
                $jQuery('.currency_change_option').click(function () {
                  changeCountry($jQuery(this).attr('data-country'))
                })
              })
            } else {
              swal('更换失败！', '', 'error')
            }
          },
          error: () => {
            swal('获取当前国家/地区失败！', '', 'error')
          }
        })
      }
    })
  }

  function redeemSubs () {
    redeemSub($jQuery('#inputKey').val().trim())
  }

  function mouseClick ($, e) {
    const $i = $('<span/>').text('Steam Key')
    const x = e.pageX
    const y = e.pageY
    $i.css({ 'z-index': 9999999999999999999, top: y - 20, left: x, position: 'absolute', 'font-weight': 'bold', color: '#ff6651' })
    $('body').append($i)
    $i.animate({ top: y - 180, opacity: 0 }, 1500, () => { $i.remove() })
  }
  function addBtn () {
    const div = document.createElement('div')
    div.setAttribute('id', 'keyDiv')
    div.setAttribute('style', 'position:fixed;left:5px;bottom:5px')
    const btn = document.createElement('button')
    btn.setAttribute('id', 'allKey')
    btn.setAttribute('key', '')
    btn.setAttribute('style', 'display:none;z-index:9999')
    btn.setAttribute('class', 'btn btn-default')
    btn.innerText = '激活本页面所有key(共0个)'
    btn.onclick = function () {
      const setting = GM_getValue('setting')
      const keys = getKeysByRE($jQuery(this).attr('key'))
      if (setting.asf) asfRedeem('!redeem ' + setting.asfBot + ' ' + keys.join(','))
      else if (setting.newTab) window.open('https://store.steampowered.com/account/registerkey?key=' + keys.join(','), '_blank')
      else webRedeem(keys)
    }
    $jQuery('body').append(div)
    div.appendChild(btn)
    return btn
  }
  function redeemAllKey () {
    let len = 0
    let keyList = ''
    let hasKey = []
    const btn = addBtn()
    setInterval(function () {
      const allSteamKey = arr(getKeysByRE($jQuery('body').text())) || []
      len = allSteamKey.length
      if (len > 0) {
        hasKey.push(...allSteamKey)
        hasKey = arr(hasKey)
        keyList = hasKey.join(',')
        if ($jQuery(btn).attr('key') !== keyList) {
          $jQuery(btn).attr('key', keyList)
          $jQuery(btn).text('激活本页面所有key(共' + hasKey.length + '个)')
          $jQuery(btn).show()
        }
      } else if (document.getElementById('allKey') && (document.getElementById('allKey').style.display === 'block')) {
        $jQuery(btn).hide()
        $jQuery(btn).text('激活本页面所有key(共0个)')
      }
    }, 1000)
  }
  function arr (arr) {
    return [...new Set(arr)]
  }
}($))
