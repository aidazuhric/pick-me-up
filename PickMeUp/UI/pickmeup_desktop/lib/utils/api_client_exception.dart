import 'dart:convert';

class ApiClientException implements Exception {
  ApiClientException(this.message, {this.isUnauthorized = false});

  final String message;
  final bool isUnauthorized;

  @override
  String toString() => message;
}

class ApiErrorParser {
  ApiErrorParser._();

  static String? messageFromBody(String body) {
    final trimmed = body.trim();
    if (trimmed.isEmpty) {
      return null;
    }

    try {
      final decoded = jsonDecode(trimmed);
      if (decoded is! Map) {
        return null;
      }

      final map = Map<String, dynamic>.from(decoded);

      final root = map['message'];
      if (root is String && root.trim().isNotEmpty) {
        return root.trim();
      }

      final errors = map['errors'];
      if (errors is Map) {
        final errMap = Map<String, dynamic>.from(errors);
        final parts = <String>[];

        parts.addAll(_messagesFromValue(errMap['clientError']));
        for (final key in errMap.keys) {
          if (key != 'clientError') {
            parts.addAll(_messagesFromValue(errMap[key]));
          }
        }

        final unique = parts.toSet().toList();
        if (unique.isNotEmpty) {
          return unique.join('\n');
        }
      }

      return null;
    } catch (_) {
      return null;
    }
  }

  static List<String> _messagesFromValue(dynamic value) {
    if (value is List) {
      return value
          .map((e) => e.toString().trim())
          .where((s) => s.isNotEmpty)
          .toList();
    }
    if (value is String && value.trim().isNotEmpty) {
      return [value.trim()];
    }
    return [];
  }
}
