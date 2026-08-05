import 'package:flutter/material.dart';

/// Placeholder for the coordinate picker. Coordinates must be chosen on a map or resolved
/// from an address, never typed in, so no text entry is offered here until the map provider
/// is wired up.
class MapPickerModal extends StatelessWidget {
  const MapPickerModal({super.key});

  static Future<void> show(BuildContext context) {
    return showDialog<void>(context: context, builder: (_) => const MapPickerModal());
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Pick a location'),
      content: const Text(
        'Map selection is not available yet. Coordinates will be chosen on the map or resolved '
        'from an address once the map provider is connected.',
      ),
      actions: [
        TextButton(onPressed: () => Navigator.of(context).pop(), child: const Text('CLOSE')),
      ],
    );
  }
}
