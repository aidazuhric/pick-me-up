import 'package:flutter/material.dart';

import '../core/theme/app_colors.dart';

class AppMessages {
  AppMessages._();

  static void success(BuildContext context, String message) {
    _show(context, message, AppColors.toggleOn);
  }

  static void failure(BuildContext context, String message) {
    _show(context, message, AppColors.danger);
  }

  static void _show(BuildContext context, String message, Color background) {
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(
        SnackBar(
          content: Text(message),
          backgroundColor: background,
          behavior: SnackBarBehavior.floating,
          duration: const Duration(seconds: 4),
        ),
      );
  }
}
