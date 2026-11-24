// ui.js — رفتار کوچک رابط: منوی موبایل، مگامنو، مودال، بستن پیام. بدون وابستگی بیرونی.
(function () {
  "use strict";

  // منوی موبایل
  document.querySelectorAll("[data-nav-toggle]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var list = document.getElementById(btn.getAttribute("aria-controls"));
      if (!list) { return; }
      var open = list.classList.toggle("is-open");
      btn.setAttribute("aria-expanded", open ? "true" : "false");
    });
  });

  // مگامنوی دسته‌بندی موضوعات
  var menuButtons = document.querySelectorAll("[data-menu-toggle]");
  function closeMenus() {
    menuButtons.forEach(function (btn) {
      var panel = document.getElementById(btn.getAttribute("aria-controls"));
      if (panel) { panel.hidden = true; }
      btn.setAttribute("aria-expanded", "false");
    });
  }
  menuButtons.forEach(function (btn) {
    btn.addEventListener("click", function (e) {
      e.stopPropagation();
      var panel = document.getElementById(btn.getAttribute("aria-controls"));
      if (!panel) { return; }
      var willOpen = panel.hidden;
      closeMenus();
      panel.hidden = !willOpen;
      btn.setAttribute("aria-expanded", willOpen ? "true" : "false");
    });
  });
  document.addEventListener("click", function (e) {
    if (!e.target.closest(".mega")) { closeMenus(); }
  });
  document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") { closeMenus(); }
  });

  // مودال (عنصر dialog)
  document.querySelectorAll("[data-modal-open]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var dlg = document.getElementById(btn.getAttribute("data-modal-open"));
      if (dlg && typeof dlg.showModal === "function") { dlg.showModal(); }
    });
  });
  document.querySelectorAll("[data-modal-close]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var dlg = btn.closest("dialog");
      if (dlg) { dlg.close(); }
    });
  });
  document.querySelectorAll("dialog.modal").forEach(function (dlg) {
    dlg.addEventListener("click", function (e) {
      if (e.target === dlg) { dlg.close(); }
    });
  });

  // بستن پیام
  document.querySelectorAll("[data-alert-dismiss]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var alert = btn.closest(".alert");
      if (alert) { alert.remove(); }
    });
  });
})();
