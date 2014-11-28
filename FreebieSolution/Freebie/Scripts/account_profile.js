function openPopup(path) {
    var wd = $('body').width();
    var wd_adjust = wd / 2.0 - 350;
    myWindow = window.open(path, '', 'width=700,height=500,top=100,left= ' + wd_adjust);
    myWindow.focus();
}

$(document).ready(function () {
    jQuery.noConflict();
    load_inputs();
    calculate();
    function get_json_data() {
        var json = null;
        $.ajax({
            'async': false,
            'global': false,
            'url': base_url + 'Users/FindAreacode',
            'dataType': "json",
            'success': function (data) {
                json = data;
            }

        })
        return json;
    }
    var area_datas = get_json_data();


    $.widget("custom.combobox", {
        _create: function () {
            this.wrapper = $("<span>")
          .addClass("custom-combobox")
          .insertAfter(this.element);

            this.element.hide();
            this._createAutocomplete();
            this._createShowAllButton();
        },

        _createAutocomplete: function () {
            var selected = this.element.children(":selected"),
          value = selected.val() ? selected.text() : "";

            this.input = $("<input>")
          .appendTo(this.wrapper)
          .val(value)
          .attr("title", "")
          .addClass("custom-combobox-input ui-widget ui-widget-content ui-state-default ui-corner-left")
          .autocomplete({
              delay: 0,
              minLength: 0,
              source: $.proxy(this, "_source")
          })
          .tooltip({
              tooltipClass: "ui-state-highlight"
          });

            this._on(this.input, {
                autocompleteselect: function (event, ui) {
                    ui.item.option.selected = true;
                    this._trigger("select", event, {
                        item: ui.item.option
                    });
                },

                autocompletechange: "_removeIfInvalid"
            });
        },

        _createShowAllButton: function () {
            var input = this.input,
          wasOpen = false;

            $("<a>")
          .attr("tabIndex", -1)
          .attr("title", "แสดงทั้งหมด")
          .tooltip()
          .appendTo(this.wrapper)
          .button({
              icons: {
                  primary: "ui-icon-triangle-1-s"
              },
              text: false
          })
          .removeClass("ui-corner-all")
          .addClass("custom-combobox-toggle ui-corner-right")
          .mousedown(function () {
              wasOpen = input.autocomplete("widget").is(":visible");
          })
          .click(function () {
              input.focus();

              // Close if already visible
              if (wasOpen) {
                  return;
              }

              // Pass empty string as value to search for, displaying all results
              input.autocomplete("search", "");
          });
        },

        _source: function (request, response) {
            var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
            response(this.element.children("option").map(function () {
                var text = $(this).text();
                if (this.value && (!request.term || matcher.test(text)))
                    return {
                        label: text,
                        value: text,
                        option: this
                    };
            }));
        },

        _removeIfInvalid: function (event, ui) {

            // Selected an item, nothing to do
            if (ui.item) {
                return;
            }

            // Search for a match (case-insensitive)
            var value = this.input.val(),
          valueLowerCase = value.toLowerCase(),
          valid = false;
            this.element.children("option").each(function () {
                if ($(this).text().toLowerCase() === valueLowerCase) {
                    this.selected = valid = true;
                    return false;
                }
            });

            // Found a match, nothing to do
            if (valid) {
                return;
            }

            // Remove invalid value
            this.input
          .val("")
          .attr("title", value + " ไม่พบข้อมูล")
          .tooltip("open");
            this.element.val("");
            this._delay(function () {
                this.input.tooltip("close").attr("title", "");
            }, 2500);
            this.input.data("ui-autocomplete").term = "";
        },

        _destroy: function () {
            this.wrapper.remove();
            this.element.show();
        }
    });

    $('#AreaCode').combobox(
            {
                select: function (event, ui) {
                    var area_code = ui.item.value;
                    var selected_zipcode = "";

                    $.each(area_datas, function (i, data) {
                        if (data.AreaCode == area_code) {
                            selected_zipcode = data.ZipCode;
                        }
                    });
                    $('#ZipCode').val(selected_zipcode);
                }
            }
        );

    $('#ZipCode').on('blur', function (e) {
        var zipcode = $(e.target).val();
        var json = null;
        $('input.custom-combobox-input').val('');
        $.ajax({
            'async': false,
            'global': false,
            'url': base_url + 'Users/FindAreacode',
            'dataType': "json",
            data: { zipcode: zipcode },
            'success': function (data) {
                json = data;
                var select_combo = $('#AreaCode');
                var string_html = "<option value>-</option>";
                $.each(json, function (i, area) {
                    string_html += "<option value='" + area.AreaCode + "'>" + area.District + " </option>"
                });
                select_combo.html(string_html);
                // $('#AreaCode').combobox();
            }

        });

    });
    

    $('#ZipCode').keydown(function (e) {
        var keys = [8, 9, 16, 17, 18, 19, 20, 27, 33, 34, 35, 36, 37, 38, 39, 40, 45, 46, 144, 145];

        if ($.inArray(e.keyCode, keys) == -1) {
            if (checkMaxLength($(this).val(), 5)) {
                e.preventDefault();
                e.stopPropagation();
            }
        }

    });

    $('#Identification_Number').keydown(function (e) {
        var keys = [8, 9, 16, 17, 18, 19, 20, 27, 33, 34, 35, 36, 37, 38, 39, 40, 45, 46, 144, 145];

        if ($.inArray(e.keyCode, keys) == -1) {
            if (checkMaxLength($(this).val(), 13)) {
                e.preventDefault();
                e.stopPropagation();
            }
        }

    });
    function checkMaxLength(text, max) {
        return (text.length >= max);
    }
    if ($('#Children_Flag_False').is(':checked') || $('#Children_Flag_True').not(':checked')) {
        $('.select-child-year').hide();
    }
    if ($('#Children_Flag_True').is(':checked')) {
        $('.select-child-year').show();
    }
    $('#eula').on('click', function (e) {
        openPopup(base_url + 'Users/EULA');
        return false;
    });
    $('#policy').on('click', function (e) {
        openPopup(base_url + 'Users/IndividualPolicy');
        return false;           
    });

    $('#Children_Flag_False').on('click', function (e) {
        $('.select-child-year').hide();
    });
    $('#Children_Flag_True').on('click', function (e) {
        $('.select-child-year').show();
    });
});