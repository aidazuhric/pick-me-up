import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';
import '../../providers/support_ticket_provider.dart';
import '../../utils/api_client_exception.dart';

class SupportTicketFormScreen extends StatefulWidget {
  const SupportTicketFormScreen({super.key});

  static const int minSubjectLength = 5;
  static const int maxSubjectLength = 150;
  static const int minBodyLength = 10;
  static const int maxBodyLength = 2000;

  static Future<bool?> open(BuildContext context) {
    return Navigator.of(context).push<bool>(
      MaterialPageRoute<bool>(builder: (_) => const SupportTicketFormScreen()),
    );
  }

  @override
  State<SupportTicketFormScreen> createState() =>
      _SupportTicketFormScreenState();
}

class _SupportTicketFormScreenState extends State<SupportTicketFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _subjectController = TextEditingController();
  final _bodyController = TextEditingController();
  final _tickets = SupportTicketProvider();

  bool _isSubmitting = false;
  String? _submitError;

  @override
  void dispose() {
    _subjectController.dispose();
    _bodyController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() => _submitError = null);

    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSubmitting = true);

    final navigator = Navigator.of(context);

    try {
      await _tickets.insert({
        'subject': _subjectController.text.trim(),
        'body': _bodyController.text.trim(),
      });

      navigator.pop(true);
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _submitError = e.message;
        _isSubmitting = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _submitError =
            'Could not reach the server. Check that the API is running.';
        _isSubmitting = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Ask for help')),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: const EdgeInsets.all(AppSpacing.screenPadding),
          children: [
            Text(
              'Tell us what went wrong and the support team will answer here.',
              style: theme.textTheme.bodySmall,
            ),
            const SizedBox(height: AppSpacing.lg),
            TextFormField(
              controller: _subjectController,
              enabled: !_isSubmitting,
              maxLength: SupportTicketFormScreen.maxSubjectLength,
              textInputAction: TextInputAction.next,
              decoration: const InputDecoration(
                hintText: 'Subject',
                prefixIcon: Icon(Icons.subject),
              ),
              validator: (value) {
                final text = (value ?? '').trim();

                if (text.isEmpty) {
                  return 'Subject is required.';
                }
                if (text.length < SupportTicketFormScreen.minSubjectLength) {
                  return 'Subject must be at least '
                      '${SupportTicketFormScreen.minSubjectLength} characters.';
                }
                return null;
              },
            ),
            const SizedBox(height: AppSpacing.sm),
            TextFormField(
              controller: _bodyController,
              enabled: !_isSubmitting,
              maxLines: 6,
              maxLength: SupportTicketFormScreen.maxBodyLength,
              decoration: const InputDecoration(
                hintText: 'Describe what happened',
                alignLabelWithHint: true,
              ),
              validator: (value) {
                final text = (value ?? '').trim();

                if (text.isEmpty) {
                  return 'Describe the problem so we can help.';
                }
                if (text.length < SupportTicketFormScreen.minBodyLength) {
                  return 'Description must be at least '
                      '${SupportTicketFormScreen.minBodyLength} characters.';
                }
                return null;
              },
            ),
            if (_submitError != null)
              Padding(
                padding: const EdgeInsets.only(bottom: AppSpacing.sm),
                child: Text(
                  _submitError!,
                  style: TextStyle(
                    color: theme.colorScheme.error,
                    fontSize: 13,
                  ),
                ),
              ),
            const SizedBox(height: AppSpacing.sm),
            FilledButton(
              onPressed: _isSubmitting ? null : _submit,
              child: _isSubmitting
                  ? const SizedBox(
                      height: 20,
                      width: 20,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : const Text('SEND TO SUPPORT'),
            ),
          ],
        ),
      ),
    );
  }
}
