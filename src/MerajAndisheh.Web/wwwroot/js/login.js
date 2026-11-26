/* login.js — فرم ورود یکپارچه: وقتی ورودی شبیه شماره‌ی موبایل است، فیلد رمز پنهان می‌شود (چون کد پیامکی می‌آید).
   فقط بهبود ظاهر است؛ تصمیم اصلی سمت سرور است (PhoneNumberNormalizer.LooksLikePhone) و این تابع باید با آن یکی بماند. */
(function () {
  'use strict';

  var field = document.querySelector('[data-login-field]');
  var passwordWrap = document.querySelector('[data-password-field]');
  if (!field || !passwordWrap) return;

  function toLatinDigits(text) {
    return text
      .replace(/[\u06F0-\u06F9]/g, function (d) { return String(d.charCodeAt(0) - 0x06F0); })
      .replace(/[\u0660-\u0669]/g, function (d) { return String(d.charCodeAt(0) - 0x0660); });
  }

  function looksLikePhone(value) {
    var compact = toLatinDigits(value).replace(/[\s\-()\u200c\u200e\u200f]/g, '');
    if (compact.charAt(0) === '+') compact = compact.slice(1);
    return compact.length > 0 && /^[0-9]+$/.test(compact);
  }

  function sync() {
    passwordWrap.hidden = looksLikePhone(field.value);
  }

  field.addEventListener('input', sync);
  sync();
})();
