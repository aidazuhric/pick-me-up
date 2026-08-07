import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';
import '../../providers/support_ticket_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/validators.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/labeled_form.dart';

class SupportTicketForm extends StatefulWidget {
  const SupportTicketForm({super.key, required this.provider});

  final SupportTicketProvider provider;

  static Future<bool> show(
    BuildContext context, {
    required SupportTicketProvider provider,
  }) async {
    final sent = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (_) => Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 80, vertical: 40),
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: SizedBox(
            width: 760,
            height: 560,
            child: SupportTicketForm(provider: provider),
          ),
        ),
      ),
    );

    return sent ?? false;
  }

  @override
  State<SupportTicketForm> createState() => _SupportTicketFormState();
}

class _SupportTicketFormState extends State<SupportTicketForm> {
  final _formKey = GlobalKey<FormState>();
  final _subject = TextEditingController();
  final _body = TextEditingController();

  bool _isSaving = false;

  @override
  void dispose() {
    _subject.dispose();
    _body.dispose();
    super.dispose();
  }

  Future<void> _send() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSaving = true);

    try {
      final saved = await widget.provider.insert({
        'subject': _subject.text.trim(),
        'body': _body.text.trim(),
      });

      if (!mounted) return;

      AppMessages.success(
        context,
        'We received "${saved.subject}". Support will reply here.',
      );

      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSaving = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  String? _subjectError(String? value) =>
      Validators.required(value, 'a subject') ??
      Validators.maxLength(value, 150, 'The subject');

  String? _bodyError(String? value) {
    final missing = Validators.required(value, 'a description');
    if (missing != null) return missing;

    if (value!.trim().length < 15) {
      return 'Describe the problem in at least 15 characters.';
    }

    return Validators.maxLength(value, 2000, 'The description');
  }

  @override
  Widget build(BuildContext context) {
    return LabeledForm(
      title: 'Ask for help',
      subtitle: 'Tell us what went wrong and support will reply on this screen.',
      formKey: _formKey,
      isSaving: _isSaving,
      saveLabel: 'SEND',
      onBack: _isSaving ? null : () => Navigator.of(context).pop(false),
      onSave: _send,
      fields: [
        LabeledField(
          label: 'Subject',
          isRequired: true,
          hint: 'A short summary, such as "Driver could not close a ride".',
          child: TextFormField(
            controller: _subject,
            enabled: !_isSaving,
            decoration: const InputDecoration(hintText: 'What is this about?'),
            validator: _subjectError,
          ),
        ),
        LabeledField(
          label: 'What happened',
          isRequired: true,
          isWide: true,
          hint: 'Include the ride, the driver or the vehicle if it helps.',
          child: TextFormField(
            controller: _body,
            enabled: !_isSaving,
            minLines: 6,
            maxLines: 10,
            decoration: const InputDecoration(
              hintText: 'Describe the problem in your own words',
            ),
            validator: _bodyError,
          ),
        ),
      ],
    );
  }
}
