// Layout - Auto-highlight the active nav link based on current path
$(document).ready(function () {
    let currentPath = window.location.pathname.toLowerCase();
    if (currentPath === "/" || currentPath === "") {
        currentPath = "/home/index";
    }
    $(".nav-link-premium").each(function () {
        let href = $(this).attr("href").toLowerCase();
        if (currentPath.startsWith(href)) {
            $(this).closest(".nav-item").addClass("active");
        }
    });
});
