// ----------
// DanAjax.js
// ----------
// Provides a function to assign an onclick handler to
// $(this) HTML element.  Call this function
// directly on <a>'s to force the link to load
// asynchronously with Ajax into the ajaxStuff.body <div>

var ajaxStuff = {
    body: "",
    loadCont: "",
    navbar: ""
};

var clickAjaxOverride = function () {
    $(this).click(function (e) {

        // Stop the link from changing document.location
        e.preventDefault();

        var href = $(this).attr('href');

        // Toggle loading container with ajax call
        $(document).ajaxStart(function () {
          $(ajaxStuff.body).hide();
          $(ajaxStuff.loadCont).show();
        });
        $(document).ajaxStop(function () {
          $(ajaxStuff.loadCont).hide();
          $(ajaxStuff.body).show();
        });
        $(document).ajaxError(function(event, request, settings, error) {
          var msg = "<h1 class='text-danger'>Error</h1>";
          msg += "<h3>"+ request.status +" <small>"+ error +"</small></h3>";
          msg += "<p>Unable to retrieve <em>"+ settings.url +"</em>.</p>";
          
          $(ajaxStuff.body).html(msg);
          
          $(ajaxStuff.loadCont).hide();
          $(ajaxStuff.body).show();
        });

        // Perform ajax call
        $.ajax({
            url: href,
            type: 'GET',
            success: function (data) {
                $(ajaxStuff.body).html(data);
            }
        });

        if ($(this).parent().attr('data-action')) {
            // Select the appropriate active link in the navbar
            $('ul#sidebar-nav > li').removeClass('active');
            $('ul#sidebar-nav > li > ul.collapse > li').removeClass('active');
            $(this).parent().addClass('active');
        }
    });
};