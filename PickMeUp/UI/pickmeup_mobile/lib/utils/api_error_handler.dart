import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../providers/auth_provider.dart';
import '../widgets/app_messages.dart';
import 'api_client_exception.dart';

class ApiErrorHandler {
  ApiErrorHandler._();

  static void surface(BuildContext context, Object error) {
    if (error is ApiClientException) {
      if (error.isUnauthorized) {
        AppMessages.failure(context, error.message);
        context.read<AuthProvider>().clearSession();
        return;
      }

      AppMessages.failure(context, error.message);
      return;
    }

    AppMessages.failure(
      context,
      'Could not reach the server. Check that the API is running.',
    );
  }
}
