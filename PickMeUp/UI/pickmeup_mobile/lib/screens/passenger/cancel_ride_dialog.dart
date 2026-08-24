import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';

class CancelRideDialog extends StatefulWidget {
  const CancelRideDialog({super.key});

  static const int maxReasonLength = 500;

  static Future<String?> show(BuildContext context) {
    return showDialog<String>(
      context: context,
      builder: (_) => const CancelRideDialog(),
    );
  }

  @override
  State<CancelRideDialog> createState() => _CancelRideDialogState();
}

class _CancelRideDialogState extends State<CancelRideDialog> {
  final _formKey = GlobalKey<FormState>();
  final _reasonController = TextEditingController();

  @override
  void dispose() {
    _reasonController.dispose();
    super.dispose();
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    Navigator.of(context).pop(_reasonController.text.trim());
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return AlertDialog(
      title: const Text('Cancel this ride?'),
      content: Form(
        key: _formKey,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(
              'Tell us why so the driver and the company know what happened.',
              style: theme.textTheme.bodySmall,
            ),
            const SizedBox(height: AppSpacing.md),
            TextFormField(
              controller: _reasonController,
              autofocus: true,
              maxLines: 3,
              maxLength: CancelRideDialog.maxReasonLength,
              decoration: const InputDecoration(
                hintText: 'Reason for cancelling',
              ),
              validator: (value) => (value ?? '').trim().isEmpty
                  ? 'A reason is required when cancelling a ride.'
                  : null,
            ),
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('KEEP THE RIDE'),
        ),
        FilledButton(
          onPressed: _submit,
          child: const Text('CANCEL RIDE'),
        ),
      ],
    );
  }
}
