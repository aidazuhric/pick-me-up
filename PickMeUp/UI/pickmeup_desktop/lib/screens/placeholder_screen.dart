import 'package:flutter/material.dart';

class PlaceholderScreen extends StatelessWidget {
  const PlaceholderScreen({
    super.key,
    required this.title,
    required this.arrivesWith,
  });

  final String title;
  final String arrivesWith;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(title, style: theme.textTheme.headlineSmall),
          const SizedBox(height: 12),
          Text(
            'This screen is built once $arrivesWith exists in the API.',
            style: theme.textTheme.bodyMedium,
          ),
        ],
      ),
    );
  }
}
