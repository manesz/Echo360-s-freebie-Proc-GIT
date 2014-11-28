var fname, lname, dob, mob, yob, gm, gf, ms, ir, pir, o, e, cft, cff, yobc1, yobc2, yobc3, idn = null;
var interest_arrs = [];
var loq_quota, medium_quota, high_quota = null;
var pfreq = null;
var pdur = null;
var mark_min, mark_mid, mark_max = null;
var img = null;
var mark_min_val, mark_mid_val, mark_max_val = null

var calculate = function () {
    var step1 = false;
    var step2 = false;
    var step3 = false;
    var score = 0;
    if ($.trim(fname.val()) != "") {
        if ($.trim(lname.val()) != "") {
            score = 1;
            if (dob.val() != "") {
                if (mob.val() != "") {
                    if (yob.val() != "") {
                        score = 2;
                        if (gm.is(":checked") || gf.is(":checked")) {
                            score = 3;
                            step1 = true;
                        }
                    }
                }
            }
        }
    }

    if (step1) {
        if (ir.val() != "") {

            if (o.val() != "") {
                score = 4;
                if (e.val() != "") {
                    score = 5;
                    if (ms.val() != "") {
                        if (cft.is(":checked") || cff.is(":checked")) {
                            score = 6;
                            step2 = true;
                        }

                    }

                }
            }

        }
    }

    if (step1 && step2) {

        //if (yobc1.val() != "" || yobc2.val() != "" || yobc3.val() != "") {
        if ($.trim(idn.val()) != "") {
            score = 7;
            if ($('input:checkbox').not('#eula-checkbox').filter(':checked').size() > 0) {
                score = 8;
                step3 = true;
            }
        }
        // }
    }

    var src = base_url + "Images/register/" + "dial_" + score + ".png"
    img.attr("src", src);


    if (step1 && step2 && step3) {
        pfreq.text(high_quota['freq']);
        pdur.text(high_quota['dur']);

    } else {
        if (step1 && step2) {
            pfreq.text(medium_quota['freq']);
            pdur.text(medium_quota['dur']);
        } else {
            if (step1) {
                pfreq.text(low_quota['freq']);
                pdur.text(low_quota['dur']);
            } else {

                pfreq.text("0");
                pdur.text("0");
            }
        }
    }



}

var add_interest = function () {
    var selected_interests = $('input:checkbox').not('#eula-checkbox').filter(':checked');
}

var load_inputs = function () {
    fname = $('input#First_Name');
    lname = $('input#Last_Name');
    dob = $('select#Day_Of_Birth');
    mob = $('select#Month_Of_Birth');
    yob = $('select#Year_Of_Birth');
    gm = $('input#Gender_Cd_M');
    gf = $('input#Gender_Cd_F');
    ms = $('select#Marital_Status_Cd');
    ir = $('select#Income_Range_Cd');
    pir = $('select#Personal_Income_Range_Cd');
    o = $('select#Occupation_Cd');
    e = $('select#Education_Cd');
    cft = $('input#Children_Flag_True');
    cff = $('input#Children_Flag_False');
    yobc1 = $('select#Year_Of_Birth_Child1');
    yobc2 = $('select#Year_Of_Birth_Child2');
    yobc3 = $('select#Year_Of_Birth_Child3');
    idn = $('input#Identification_Number');

    mark_min = $('#mark-min-quota');
    mark_mid = $('#mark-mid-quota');
    mark_max = $('#mark-max-quota');
    img = $('#meter-gauge-img');

    low_quota = {
        "freq": $("li#low").attr('data-freq-val'),
        "dur": $("li#low").attr('data-dur-val')
    };
    medium_quota = {
        "freq": $("li#medium").attr('data-freq-val'),
        "dur": $("li#medium").attr('data-dur-val')
    };
    high_quota = {
        "freq": $("li#high").attr('data-freq-val'),
        "dur": $("li#high").attr('data-dur-val')
    };

    pfreq = $('#preview-quota-freq');
    pdur = $('#preview-quota-dur');

    pfreq.text("0");
    pdur.text("0");

    mark_min_val = low_quota["dur"] * low_quota["freq"] * 30;
    mark_mid_val = medium_quota["dur"] * medium_quota["freq"] * 30;
    mark_max_val = high_quota["dur"] * high_quota["freq"] * 30;

    mark_min.text(mark_min_val);
    mark_mid.text(mark_mid_val);
    mark_max.text(mark_max_val);

    $('input:text').on('blur', function (e) {
        calculate();
    })

    $('input:radio').on('change', function (e) {
        calculate();
    })
    $('select').on('change', function (e) {
        calculate();
    })

    $('input:checkbox').on('change', function (e) {
        calculate();
    })

}