// ========================
// .ellipsis() jQuery addon
// ========================
(function($) {
  $.fn.ellipsis = function()
  {
    return this.each(function()
    {
      var el = $(this);

      if(el.css("overflow") == "hidden")
      {
        var text = el.html();
        var multiline = el.hasClass('multiline');
        var t = $(this.cloneNode(true))
                .hide()
                .css('position', 'absolute')
                .css('overflow', 'visible')
                .width(multiline ? el.width() : 'auto')
                .height(multiline ? 'auto' : el.height());

        el.after(t);

        function height() { return t.height() > el.height(); };
        function width() { return t.width() > el.width(); };

        var func = multiline ? height : width;

        while (text.length > 0 && func()){
          text = text.substr(0, text.length - 1);
          t.html(text + "&hellip;");
        }

        el.html(t.html());
        t.remove();
      }
    });
  };
})(jQuery);

// ================
// Global variables
// ================
var TDAY = new Date();                                                  // Today's date
var MONTHSTARTSON = new Date(TDAY.getFullYear(), TDAY.getMonth(), 1);   // First day of month
var MONTHENDSON = new Date(TDAY.getFullYear(), TDAY.getMonth() + 1, 0); // Last day of month
var CURRENTDATE = MONTHSTARTSON;                                        // Date currently being processed
var WEEKNUMBER = 0;                                                     // The week of the month CURRENTDATE occurs in
var ORIGCONTENT1 = '';
var ORIGCONTENT2 = '';
// Arrays of names for convenience
var MONTHS = ["Janurary", "February", "March", "April", "May", "June", "July", "August",  
                "September", "October", "November", "December"];
var SHORTMONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];               
var WKDAYS = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
var SHORTWKDAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];                

// ============================
// Array of appointment objects
// ============================
// Each appointment should consist of these properties:
//    * title         * description
//    * startDate     * endDate
//    * url
var APPTS = [
  /*
  {title: "Z7H457MS",
    desc: "This is a description.",
    startDate: new Date(2014, 01, 13),
    endDate: new Date(2014, 01, 22),
    url: "/Mgmt/RecordDetail/1"
    }
  */
];

// ========
// Calendar
// ========
// Function to draw calendar of the current month
function Calendar(newDate) {
  var msg = '';                                                           // Final output string
  var dayIndex = 1;                                                       // Day of week being processed
  var monthStarted = false;                                               // Has the month started?
  var container = document.getElementById("calendarContainer");           // HTML Element to print calendar to
  var header = document.getElementById("calendarHeader");                 // HTML Element to print header to
  
  if (newDate != null && typeof newDate.getDate() == "number"){
    TDAY = newDate;
    // Reinitialize globals that depend on TDAY
    MONTHSTARTSON = new Date(TDAY.getFullYear(), TDAY.getMonth(), 1);   // First day of month
    MONTHENDSON = new Date(TDAY.getFullYear(), TDAY.getMonth() + 1, 0); // Last day of month
    CURRENTDATE = MONTHSTARTSON;                                        // Date currently being processed
    WEEKNUMBER = 0;                                                     // The week of the month CURRENTDATE occurs in
  }
  
  // Build days of current week
  function DayLoop(i) {
    var today = new Date();
    for (j = 0; j < 7; j++){
    
      msg += '<div class="day ';
      
      if (TDAY.getYear() == today.getYear() &&
          TDAY.getMonth() == today.getMonth() &&
          dayIndex == today.getDate() && 
          (j == MONTHSTARTSON.getDay() || monthStarted == true)){
        // Red BG color for today
        msg +=  'today '; 
      }
      else if (dayIndex == TDAY.getDate() && (j == MONTHSTARTSON.getDay() || monthStarted == true)){
        // Red border color for TDAY
        msg +=  'selectedDay '; 
      };
      if (j == 0 || j == 6){
        // Dark gray BG for weekend day
        msg +=  'weekend'
      };
      msg += '" data-day="'+ dayIndex +'">';
      
      if(j == MONTHSTARTSON.getDay() || monthStarted == true){
        monthStarted = true;  // The month has started
        if (dayIndex > MONTHENDSON.getDate()){
          // We've exceeded the number of days in the month
          msg +=  "&nbsp;";
        }
        else{
          msg +=  dayIndex + "<br><small>";
          msg +=  Appointments.day();
          msg +=  "</small>"; // End of appointments block
          dayIndex++;
        };
        CURRENTDATE = new Date(TDAY.getFullYear(), TDAY.getMonth(), dayIndex);
      };
      msg +=  "</div>";   // End of day cell
      
      // If we're at the end of the loops and haven't made it to the last day of the month, 
      // add one more week to finish it off.
      if (i == 4 && j == 6 && CURRENTDATE <= MONTHENDSON)
      {
        msg +=  '</div>'; // End of week row
        WEEKNUMBER++;
        
        // Start the new week
        msg +=  '<div class="wk" id="Week'+ (i+1) +'">';
        DayLoop(i+1);
      }
    }
  }
  
  // Build weeks of current month
  for (i = 0; i < 5; i++){
    msg +=  '<div class="wk">';
    
    DayLoop(i);
    
    msg +=  '</div>'; // End of week row
    
    WEEKNUMBER++;
  };
  
  CURRENTDATE = MONTHSTARTSON;
  
  // Append output to relative DOM elements
  header.innerHTML = "<h1>" + MONTHS[TDAY.getMonth()] + " " + TDAY.getFullYear() + "</h1>" + ORIGCONTENT2;
  container.innerHTML = ORIGCONTENT1 + msg;
  
  // Place ellipsis on long APPT titles
  $('.appt').ellipsis();
  
  // Setup event handlers
  $('#calendarContainer > .wk > .day').on('click', function(){
    var day = this.getAttribute('data-day');
    var date = new Date(TDAY.getFullYear(), TDAY.getMonth(), day);
    if ($("#popup > #calendarContainer")) {
        CalendarPopup.chooseDate(date);
    };
    Calendar(date);
  });
  
  // This is necessary each time Calendar() is called, 
  // not sure how else to handle it.
  if (clickAjaxOverride != null)
    $('.appt').each(clickAjaxOverride);
}

// ============
// Appointments
// ============
// Various functions for appointments
var Appointments = {
  appts: [],
  apptSort: function (appt1, appt2) {
    if (appt1.startDate > appt2.startDate) return 1;
    if (appt1.startDate < appt2.startDate) return -1;
    return 0;
  },
  dateString: function (date, lng) {
    // date == Date() object
    // lng  == boolean value
    var msg = '';
    
    if (typeof date != 'object'){
      return false;
    }
    else {
      var hours = date.getHours() > 12 ? date.getHours() - 12 : date.getHours();
      var minutes = date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
      var ampm = date.getHours() >= 12 ? "pm" : "am";
      hours = date.getHours() == 0 ? 12 : hours;
      
      if (lng == true){
        var year = date.getFullYear() == TDAY.getFullYear() ? "" : date.getFullYear();
        
        msg += WKDAYS[date.getDay()] + ", " + MONTHS[date.getMonth()] + " ";
        msg += date.getDate() + " " + year + " ";
        msg += "at " + hours + ":" + minutes + ampm;
      }
      else {
        var year = date.getFullYear() == TDAY.getFullYear() ? "" : date.getFullYear();
        var day = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();
        
        year = year.toString();
        msg += SHORTWKDAYS[date.getDay()] + " " + SHORTMONTHS[date.getMonth()] + " ";
        msg += day + " " + year.substr(2,2);
        msg += " at " + hours + ":" + minutes + ampm;
      };
      
      return msg;
    }
  },
  // Not sure if we're going to use this or not...
  switchModalBody: function(select, index) {
    switch (select) {
      case 1:
        // Show #apptModalBody
        $('#apptModalBody').css('display', 'block');
        $('#editFormCont').css('display', 'none');
        $('#submitMsg').css('display', 'none');
        // Show Edit button
        $('#delBtn').css('display', 'none');
        $('#editBtn').css('display', 'inline');
        $('#editBtn').html('Edit');
        $('#editBtn').attr('onclick', 'Appointments.edit('+ index +')');
        break;
      
      case 2:
        // Show #editFormCont for editing appt
        $('#apptModalBody').css('display', 'none');
        $('#editFormCont').css('display', 'block');
        $('#submitMsg').css('display', 'none');
        
        // Show Save Changes and Delete buttons
        $('#delBtn').css('display', 'inline');
        $('#delBtn').attr('onclick', 'Appointments.delete('+index+')');
        $('#editBtn').css('display', 'inline');
        $('#editBtn').html('Save Changes');
        $('#editBtn').attr('onclick', 'Appointments.submit(false, '+index+')');
        break;
        
      case 3:
        // Show #submitMsg
        $('#apptModalBody').css('display', 'none');
        $('#editFormCont').css('display', 'none');
        $('#submitMsg').css('display', 'block');
        // Hide Save Changes button
        $('#editBtn').css('display', 'none');
        $('#delBtn').css('display', 'none');
        break;
      
      case 4:
        // Show #editFormCont for adding appt
        $('#apptModalBody').css('display', 'none');
        $('#editFormCont').css('display', 'block');
        $('#submitMsg').css('display', 'none');
        
        // Show Save Changes button only
        $('#delBtn').css('display', 'none');
        $('#editBtn').css('display', 'inline');
        $('#editBtn').html('Save Changes');
        $('#editBtn').attr('onclick', 'Appointments.submit(true)');
        break;
    
      default:
        return false;
        break;
    };
  },
  week: function() {
    // Not using this right now...
    var ns = Appointments;
    var msg = "";
    
    ns.appts = [];
    
    for (l = 0; l < APPTS.length; l++){
      if (APPTS[l].startDate.getFullYear() == CURRENTDATE.getFullYear() &&
        APPTS[l].startDate.getMonth() == CURRENTDATE.getMonth() && 
        APPTS[l].startDate >= CURRENTDATE && 
        APPTS[l].startDate.getDate() < (CURRENTDATE.getDate() + 7 - CURRENTDATE.getDay()))
      {
        ns.appts.push(APPTS[l]);
        ns.appts[ns.appts.length - 1].index = l;
      }
    }
    
    ns.appts.sort(ns.apptSort);
    
    for(p = 0; p < ns.appts.length; p++){
      msg += "<small>" + ns.dateString(ns.appts[p].startDate, false) + "</small>";    
      msg += "<div class='appt'><a href='javascript: return false;' onclick='Appointments.detail(" + 
                ns.appts[p].index + ")'>" + 
                ns.appts[p].title + "</a>";
      msg += "</div>";
    }
    
    return msg;
  },
  day: function() {
    var ns = Appointments;
    var msg = "";
    
    ns.appts = [];
    
    for (k = 0; k < APPTS.length; k++) {
      if (APPTS[k].startDate.getFullYear() == CURRENTDATE.getFullYear() 
        && APPTS[k].startDate.getMonth() == CURRENTDATE.getMonth() 
        && (APPTS[k].startDate.getDate() <= CURRENTDATE.getDate() 
              && APPTS[k].endDate.getDate() >= CURRENTDATE.getDate()))
      {
        ns.appts.push(APPTS[k]);
        ns.appts[ns.appts.length - 1].index = k;
      }
    }
    
    ns.appts.sort(ns.apptSort);
    
    for(q = 0; q < ns.appts.length; q++){
      msg += "<a class='appt' href='"+ ns.appts[q].url +"'>" + 
                ns.appts[q].title + 
             "</a>"
    }
    
    return msg;
  },
  detail: function(index) {
    var ns = Appointments;
    var appt = APPTS[index];

    ns.switchModalBody(1, index);
    
    $('#apptModalTitle').html(appt.title);
    $('#showStartDate').html(ns.dateString(appt.startDate, true));
    $('#showEndDate').html(ns.dateString(appt.endDate, true));
    $('#showDescription').html("<b>Description:</b><br/>" + appt.desc);
    $('#showLocation').html(appt.loc);
    
    $('#mapCanvas').css('display', 'none');
    $('#apptModal').off('shown.bs.modal');
      
    $('#apptModal').modal('show');
  },
  add: function(date) {
    var ns = Appointments;
    var form = document.getElementById('editAppt');
    
    ns.switchModalBody(4);
    
    $('#apptModalTitle').html('<input type="text" name="title" value="New Appointment" />');
    
    DateSelector.printMonths(form.month1);
    DateSelector.printMonths(form.month2);
    
    form.month1.value = MONTHS[date.getMonth()];
    form.year1.value = date.getFullYear();
    
    form.month2.value = MONTHS[date.getMonth()];
    form.year2.value = date.getFullYear();
    
    DateSelector.printDays(form.day1, form.month1.value, form.year1.value);
    DateSelector.printDays(form.day2, form.month2.value, form.year2.value);
    
    form.day1.value = date.getDate();
    form.hour1.value = '12';
    form.minute1.value = '00';
    form.ampm1.value = 'pm';
    
    form.day2.value = date.getDate();
    form.hour2.value = '12';
    form.minute2.value = '30';
    form.ampm2.value = 'pm';
    
    form.location.value = '';
    form.showmap.checked = false;
    form.description.value = '';
    
    $('#apptModal').modal('show');
  },
  edit: function(index) {
    var ns = Appointments;
    var appt = APPTS[index];
    var form = document.getElementById('editAppt');
    var duration = 0;
    
    var cronos = function() {
      var date1, date2;
      
      date1 = ns.formToDate(form.month1.value, form.day1.value,
                            form.year1.value, form.hour1.value, form.minute1.value,
                            form.ampm1.value);
      
      date2 = ns.formToDate(form.month2.value, form.day2.value,
                            form.year2.value, form.hour2.value, form.minute2.value,
                            form.ampm2.value);
      
      if (date1 > date2){
        date2 = date1.valueOf() + duration;
        date2 = new Date(date2);
        
        ns.dateToForm(date2, form, 2);
      }
      else {
        return false;
      };
    };
    
    duration = appt.endDate - appt.startDate;
    
    $('#apptModalTitle').html('<input type="text" name="title" value="' + appt.title + '" />');
    
    ns.dateToForm(appt.startDate, form, 1);
    ns.dateToForm(appt.endDate, form, 2);
    
    form.location.value = appt.loc;
    form.showmap.checked = appt.showMap;
    form.description.value = appt.desc;
    
    form.month1.onchange = function(){
      var year = form.year1.value;
      var month = form.month1.value;
      
      DateSelector.printDays(form.day1, month, year);
      cronos();
    };
    form.month2.onchange = function(){
      var year = form.year2.value;
      var month = form.month2.value;
      
      DateSelector.printDays(form.day2, month, year);
      cronos();
    };
    
    form.day1.onchange = function(){
      var day1 = parseInt(form.day1.value);
      var day2 = parseInt(form.day2.value);
      form.day2.value = day2 < day1 ? day1 : day2;
    };
    form.day2.onchange = function(){
      var day1 = parseInt(form.day1.value);
      var day2 = parseInt(form.day2.value);
      form.day1.value = day2 < day1 ? day2 : day1;
    };
    
    ns.switchModalBody(2, index);
  },
  delete: function(index) {
    var sure = confirm('Are you sure you want to delete '+ APPTS[index].title +'?');
    if (sure) {
      APPTS.splice(index, 1);
      Calendar();
      $('#apptModal').modal('hide');
    };
  },
  submit: function(newAppt, index) {
    var ns = Appointments;
    var valid = '';
    var oldAppt = {};
    var appt = {};
    var form = document.getElementById("editAppt");
    var month1, month2, hour1, hour2;
    
    ns.switchModalBody(3, index);
    
    var setValues = function() {
      appt.title = form.title.value;
      
      // month day year hours minutes ampm
      appt.startDate = ns.formToDate(form.month1.value, form.day1.value,
                          form.year1.value, form.hour1.value, form.minute1.value,
                          form.ampm1.value);
      appt.endDate = ns.formToDate(form.month2.value, form.day2.value,
                          form.year2.value, form.hour2.value, form.minute2.value,
                          form.ampm2.value);
      
      appt.loc = form.location.value;
      appt.showMap = form.showmap.checked;
      appt.desc = form.description.value;
    };
    
    if(newAppt == true){
      var index = 0;
      
      appt.startDate = new Date();
      appt.endDate = new Date();
      
      setValues();
      
      APPTS.push(appt);
      index = APPTS.length - 1;
      
      valid = ns.validAppt(index);
      if (valid != ''){
        APPTS.splice(index, 1);
        
        $('#submitMsg').html('The appointment you submitted is <b>invalid</b> because ' + valid);
        
        setTimeout(function(){
          ns.add();
        }, 2500);
      }
      else {
        $('#apptModalTitle').html(form.title.value);
        $('#submitMsg').html('You created a new appointment!');
        
        Calendar();
          
        setTimeout(function(){
          $('#apptModal').modal('hide');
        }, 2500);
      };
    }
    else {
      appt = APPTS[index];
      
      oldAppt.title       = appt.title.toString();
      oldAppt.startDate   = new Date(appt.startDate.valueOf());
      oldAppt.endDate     = new Date(appt.endDate.valueOf());
      oldAppt.desc        = appt.desc.toString();
      oldAppt.loc         = appt.loc.toString();
      oldAppt.showMap     = appt.showMap.valueOf();
      
      setValues();
      
      $('#apptModalTitle').html(APPTS[index].title);
      
      valid = ns.validAppt(index);
      if(valid != ''){
        appt.title      = oldAppt.title;
        appt.startDate  = oldAppt.startDate;
        appt.endDate    = oldAppt.endDate;
        appt.loc        = oldAppt.loc;
        appt.desc       = oldAppt.desc;
        appt.showMap    = oldAppt.showMap;
        
        $('#submitMsg').html('The appointment you submitted is <b>invalid</b> because ' + valid);
        
        setTimeout(function(){
          ns.edit(index);
        }, 2500);
      }
      else {
        $('#submitMsg').html('You\'ve successfully edited ' + appt.title + '!');
        
        Calendar();
        
        setTimeout(function(){
          $('#apptModal').modal('hide');
        }, 2500);
      };
    };
  },
  validAppt: function(index){
    var appt = APPTS[index];
    var error = '';
    
    if (typeof appt.startDate.getDate() == 'number' && 
          typeof appt.endDate.getDate() == 'number') {
      if (appt.startDate >= appt.endDate) {
        error = 'end date occurs before start date.';
      };
    }
    else {
      error = 'one or more dates is invalid.';
    };
    
    if (typeof appt.title != 'string' ||
          typeof appt.loc != 'string' ||
          typeof appt.desc != 'string'){
      error = 'string expected.';
    };
    
    if (typeof appt.showMap != 'boolean'){
      error = 'boolean expected.';
    }
    
    return error;
  },
  formToDate: function(month, day, year, hours, minutes, ampm) {
    // pass-in values from form elements
    var date = new Date();
    
    for(i = 0; i < MONTHS.length; i++){
      if (MONTHS[i] == month){
        month = i;
      };
    };
    
    if(ampm == 'am'){
      if(parseInt(hours) == 12){
        hours = 0;
      }
      else {
        hours = parseInt(hours);
      };
    }
    else if(ampm == 'pm'){
      if(parseInt(hours) == 12){
        hours = 12;
      }
      else {
        hours = parseInt(hours) + 12;
      };
    };
    
    date.setMonth(month);
    date.setDate(day);
    date.setFullYear(year);
    date.setHours(hours);
    date.setMinutes(minutes);
    
    return date;
  },
  dateToForm: function(date, form, num){
    var formMonthElm, formDayElm, formYearElm, formHourElm, formMinElm, formAmpmElm;
    
    var hours = date.getHours() > 12 ? date.getHours() - 12 : date.getHours();
    var minutes = date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
    var ampm = date.getHours() >= 12 ? "pm" : "am";    
    
    if(typeof num == 'number'){
      switch(num){
        case 1:
          formMonthElm = form.month1;
          formDayElm = form.day1;
          formYearElm = form.year1;
          formHourElm = form.hour1;
          formMinElm = form.minute1;
          formAmpmElm = form.ampm1;
          break;
        case 2:
          formMonthElm = form.month2;
          formDayElm = form.day2;
          formYearElm = form.year2;
          formHourElm = form.hour2;
          formMinElm = form.minute2;
          formAmpmElm = form.ampm2;
          break;
        default:
          break;
      };
    }
    else {
      formMonthElm = form.month;
      formDayElm = form.day;
      formYearElm = form.year;
      formHourElm = form.hour;
      formMinElm = form.minute;
      formAmpmElm = form.ampm;
    };
    
    DateSelector.printMonths(formMonthElm);

    formMonthElm.value = MONTHS[date.getMonth()];
    formYearElm.value = date.getFullYear();
    
    DateSelector.printDays(formDayElm, formMonthElm.value, formYearElm.value);
    
    formDayElm.value = date.getDate();
    formHourElm.value = hours;
    formMinElm.value = minutes;
    formAmpmElm.value = ampm;
  }
}

// =============
// Date Selector
// =============
// Handles the functionality of the date selection form at the top of the page
var DateSelector = {
  form: "dateSelector",
  init: function(){
    var dsForm = document.getElementById(DateSelector.form);
    
    // Add months to form
    DateSelector.printMonths(dsForm.month);
    
    // Assign form month and year values *before* printDays() has run
    dsForm.month.value = MONTHS[TDAY.getMonth()];
    dsForm.year.value = TDAY.getFullYear();
    
    DateSelector.printDays(dsForm.day, dsForm.month.value, dsForm.year.value);
    
    // Assign form day value *after* printDays() has run
    dsForm.day.value = TDAY.getDate();
    dsForm.month.onchange = function(){
      var year = dsForm.year.value;
      var month = dsForm.month.value;
      
      DateSelector.printDays(dsForm.day, month, year);
    };
  },
  printDays: function(elem, month, year) {
    // elem   = HTML select box
    // month  = string of month name
    // year   = 4 digit number of year
    var monthNum = 0;
    var endOfMonth = new Date();
    
    // Determine the number value of the chosen month
    for (n = 0; n < MONTHS.length; n++){
      if (month == MONTHS[n]){
        monthNum = n;
      }
    }
    endOfMonth = new Date(year, (monthNum + 1), 0);
    elem.innerHTML = "";
    
    for (o = 1; o <= endOfMonth.getDate(); o++){
      elem.innerHTML += "<option>" + o + "</option>";
    }
  },
  printMonths: function(elem) {
    // elem = HTML select box
    
    for (m = 0; m < MONTHS.length; m++){
      elem.innerHTML = elem.innerHTML + "<option>" + MONTHS[m] + "</option>";
    }
  },
  submit: function(){
    var form      = document.getElementById(DateSelector.form);
    var year      = parseInt(form.year.value);
    var month     = form.month.value;
    var day       = parseInt(form.day.value);
    var monthNum  = 0;
    var newDate;
    
    for (n = 0; n < MONTHS.length; n++){
      if (month == MONTHS[n]){
        monthNum = n;
      }
    }
    
    newDate = new Date(year, monthNum, day);
    
    Calendar(newDate);
  },
  returnToToday: function() {
    Calendar(new Date());
  }
};

// ==============
// Popup Calendar
// ==============
// Object to hold functionality related to the popup calendar used in forms
var CalendarPopup = {
    popup: "#popup",
    click: function (e) {
        var ns = CalendarPopup;
        var left = parseInt(e.pageX) - parseInt($("#nav").width());
        var top = parseInt(e.pageY) - 90;
        $(ns.popup).css("position", "absolute");
        $(ns.popup).css('top', top + "px");
        $(ns.popup).css('left', left + "px");
        $(ns.popup).css('display', 'inline');
    },
    dismiss: function () {
        var ns = CalendarPopup;
        $(ns.popup).css('display', 'none');
    },
    chooseDate: function (date) {
        var ns = CalendarPopup;
        if (typeof date.getDate() == "number") {
            var dateString = "";
            dateString += (date.getMonth() + 1) + "/" + date.getDate() + "/" +
                date.getFullYear();
            $('input[type="datetime"]').attr('value', dateString);
            ns.dismiss();
        }
    }
};

// =====
// Ready
// =====
// Do everything that's necessary to start the program, after the window
// finishes loading
function DancalReady() {  
  ORIGCONTENT1 = document.getElementById("calendarContainer").innerHTML;
  ORIGCONTENT2 = document.getElementById("calendarHeader").innerHTML;
  
  Calendar();
}
