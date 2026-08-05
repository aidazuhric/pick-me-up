import 'package:flutter/material.dart';

class DisabledAction extends StatelessWidget {
  const DisabledAction({
    super.key,
    required this.child,
    required this.isEnabled,
    required this.reason,
  });

  final Widget child;
  final bool isEnabled;
  final String reason;

  @override
  Widget build(BuildContext context) {
    if (isEnabled) {
      return child;
    }

    return Tooltip(
      message: reason,
      child: Opacity(
        opacity: 0.4,
        child: IgnorePointer(child: child),
      ),
    );
  }
}
