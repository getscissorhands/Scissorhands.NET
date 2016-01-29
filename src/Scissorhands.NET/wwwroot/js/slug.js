/**
 * SlugAutofill
 *
 * This code is deeply dependent on jQuery
 */

(function(global, $, undefined) {

  function SlugAutofill() {
    SlugAutofill.init.apply(this, arguments);
  }

  // Slug Parser
  var ParseRules = SlugAutofill.ParseRules = [];

  /**
   * Add a Parse Rule
   * @param {RegExp|string} rule - a rule of RegExp for parsing
   * @param {string} replace - a new text for replacement
   */
  SlugAutofill.addParseRule = function(rule, replace) {
    ParseRules.push(function(text) {
      return text.replace(rule, replace);
    });
  };

  /**
   * Parse the text via ParseRules
   * @param {string} text
   */
  SlugAutofill.parse = function(text) {
    var result = text;
    $(ParseRules).each(function(_, func) {
      result = func(result);
    });
    return result;
  };

  // add basic rules for parse
  SlugAutofill.addParseRule(/ /gi, '-');
  SlugAutofill.addParseRule(/[^\w\s\-\_]/gi, '');


  /**
   * initialize Slug Autofill elements
   * Slug field requires `trigger-field`, `lock-checkbox` attributes.
   * @constructor
   * @param {string} element - element of slug input
   */
  SlugAutofill.init = function(element) {
    var ele = $(element),
        triggerId = ele.attr('trigger-field'),
        trigger,
        lockId = ele.attr('lock-checkbox'),
        lock;

    if (! triggerId || $('#' + triggerId).length == 0) {
      throw Error('trigger field is missing');
    } else {
      trigger = $('#' + triggerId);
    }

    if (lockId && $('#' + lockId).length > 0) {
      lock = $('#' + lockId);
    } else {
      var newId = uid('slug-lock');
      lock = $('<input type="checkbox">')
                .attr('id', newId)
                .prop('checked', false)
                .insertAfter(ele);
      ele.attr('lock-checkbox', newId);
    }

    // initialize the attributes
    ele.prop('readonly', true);

    // initialize the events
    trigger.on('change', updateSlug);
    lock.on('change', clickCheckbox);
    lock.on('click', clickCheckbox);

    function updateSlug() {
      ele.val(SlugAutofill.parse($(this).val()));
    }

    function clickCheckbox() {
      ele.prop('readonly', !$(this).is(':checked'));
    }
  };


  /**
   * Global initialize via jQuery selector
   * @param {string} selector - jQuery selector. '.slug-input' set as default
   */
  SlugAutofill.globalInit = function(selector) {
    $(selector || '.slug-input').each(function() {
      if (!$(this).prop('slug')) {
        $(this).prop('slug', new SlugAutofill($(this)));
      }
    });
  };

  /**
   * uid generator (quick and dirty version)
   * @param {string} prefix
   * @return string
   */
  function uid(prefix) {
    return (prefix ? prefix + '-' : '') + Math.floor(Math.random() * 1000);
  }

  // expose the code via global scope
  global.SlugAutofill = SlugAutofill;

})(window, jQuery);
