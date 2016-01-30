var expect = chai.expect;

describe("There is a tick box right next to the slug field", function() {

  var $slugInput, checkboxId, $slugCheckbox, triggerId, $slugTrigger;

  beforeEach(function() {
    // create the inputs
    jQuery('<input type="text">')
      .addClass('subject')
      .attr('id', 'subject-for-testing')
      .appendTo(jQuery('#fixtures'));

    jQuery('<input type="text">')
      .addClass('slug-input')
      .attr('trigger-field', 'subject-for-testing')
      .appendTo(jQuery('#fixtures'));

    // global initialize
    SlugAutofill.globalInit();

    // set elements for testing
    $slugInput = jQuery('.slug-input').first();

    checkboxId = $slugInput.attr('lock-checkbox');
    triggerId = $slugInput.attr('trigger-field');

    $slugCheckbox = jQuery('#' + checkboxId);
    $slugTrigger = jQuery('#' + triggerId);
  });

  afterEach(function() {
    // remove elements
    $slugInput.remove();
    $slugTrigger.remove();
    $slugCheckbox.remove();
  });

  describe("Tickbox", function() {
    it("should be 'not checked' as default", function() {
      expect($slugCheckbox).to.have.prop('checked', false);
    });
  });
  describe("Slug field", function() {
    it("should be 'read-only' as default", function() {
      expect($slugInput).to.have.attr('readonly');
    });

    it("should have a Subject field ID", function() {
      expect($slugInput).to.have.attr('trigger-field');
    });
  });

  describe("When a user types the post/page title into the title field", function() {
    describe("auto-fills the slug field", function() {
      it('should be replacing Spaces to hypens(-)', function() {
        var text = 'Hello World';
        $slugTrigger.val(text).trigger('change');
        var result = $slugInput.val();
        expect(result).to.equal('Hello-World');
      });

      it('should be kept the Underscores(_)', function() {
        var text = 'Hello_World';
        $slugTrigger.val(text).trigger('change');
        var result = $slugInput.val();
        expect(result).to.equal('Hello_World');
      });

      it('should be kept the Hypens(-)', function() {
        var text = 'He-llo-Worl-d';
        $slugTrigger.val(text).trigger('change');
        var result = $slugInput.val();
        expect(result).to.equal('He-llo-Worl-d');
      });

      it('should be dropped all other special chararaters', function() {
        var text = 'He@-l$lo-#Wo!rl@-d';
        $slugTrigger.val(text).trigger('change');
        var result = $slugInput.val();
        expect(result).to.equal('He-llo-Worl-d');
      });
    });
  });

  describe("When the tick box is checked", function() {
    var defaultText = 'He-llo-Worl-d';

    beforeEach(function() {
      $slugTrigger.val(defaultText).trigger('change');
      $slugCheckbox.trigger('click');
    });

    afterEach(function() {

    });

    describe("Slug field", function() {
      it("should get the read-only attribute removed", function() {
        expect($slugInput).to.have.not.attr('readonly');
      });
    });
    describe("auto-filled slug", function() {
      it("should remains as default", function() {
        var result = $slugInput.val();
        expect(result).to.equal(defaultText);
      });
    });
  });
});
