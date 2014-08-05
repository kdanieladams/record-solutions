//------------
// DanGreet.js
//------------
// Greets the given user with a time sensitive greeting,
// the creates an automatically fading notification to 
// relay helpful information.
//
// Prerequisites:   
//  * jQuery 1.10+
//  * A <small> within the $(greeting) element to house 
//      the fading notification

// Replace this object before danGreetInit() is called
// to set custom options
var danGreetStuff = {
    user: "",
    greetings: [],
    greeting: null,
    colorStandard: "#fff",
    colorSuccess: "#9ff",
    colorWarning: "#ff9",
    colorDanger: "#f99"
};

function danGreetInit() {
    var now = new Date();                                   // Current date object
    var time = now.toTimeString();                          // Current time string
    var i = 0;                                              // Index for fading notifications setInterval 

    var user = danGreetStuff.user;
    var greetings = danGreetStuff.greetings;
    var greeting = danGreetStuff.greeting;
    
    // Color vars for different notification types
    var standard = danGreetStuff.colorStandard;
    var success = danGreetStuff.colorSuccess;
    var warning = danGreetStuff.colorWarning;
    var danger = danGreetStuff.colorDanger;

    // Setup a friendly greeting for the User
    if (now.getHours() > 12) {
        if (now.getHours() > 17) {
            $(greeting).html('Good evening, ' + user + '.' + $(greeting).html());
        }
        else {
            $(greeting).html('Good afternoon, ' + user + '.' + $(greeting).html());
        };
    }
    else {
        if (now.getHours() < 4) {
            $(greeting).html('Good night, ' + user + '.' + $(greeting).html());
        }
        else {
            $(greeting).html('Good morning, ' + user + '.' + $(greeting).html());
        }
    };

    // Start fading through the notifications 
    if (greetings.length > 1) {
        var varGreet = setInterval(function () {
            $(greeting +' > small').fadeOut(500, function () {
              $(greeting +' > small').css('color',
                greetings[i][1] == 4 ? danger :
                greetings[i][1] == 3 ? warning :
                greetings[i][1] == 2 ? success :
                greetings[i][1] == 1 ? standard : standard);
              $(greeting +' > small').html(greetings[i][0]);
              $(greeting +' > small').fadeIn(500);
            });
            if (i < (greetings.length - 1)) {
              i++;
            }
            else {
              i = 0;
            };
        }, 5000);
    }
    // If there's only one, no need to setInterval
    else{
      var color = greetings[0][1] == 4 ? danger :
          greetings[0][1] == 3 ? warning :
          greetings[0][1] == 2 ? success :
          greetings[0][1] == 1 ? standard : standard;
          
      $(greeting +' > small').fadeOut(500, function () {
        $(greeting +' > small').css('color', color);
        $(greeting +' > small').html(greetings[0][0]);
        $(greeting +' > small').fadeIn(500);
      });
    };
};
// End-DanGreet.js