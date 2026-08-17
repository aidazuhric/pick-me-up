import 'package:flutter/widgets.dart';

class MobileDestination {
  const MobileDestination({
    required this.icon,
    required this.label,
    required this.builder,
  });

  final IconData icon;
  final String label;
  final WidgetBuilder builder;
}
