import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

import '../core/config/api_config.dart';
import '../utils/api_client_exception.dart';

class TwoFactorChallenge {
  const TwoFactorChallenge({
    required this.maskedEmail,
    required this.expiresInSeconds,
  });

  final String maskedEmail;
  final int expiresInSeconds;
}

class AuthProvider with ChangeNotifier {
  static String? accessToken;
  static String? refreshToken;

  String? _role;
  String? _displayName;
  int? _userId;
  bool _mustChangePassword = false;

  String? get role => _role;
  String? get displayName => _displayName;
  int? get userId => _userId;
  bool get isAuthenticated => accessToken != null;
  bool get mustChangePassword => _mustChangePassword;

  void passwordChanged() {
    _mustChangePassword = false;
    notifyListeners();
  }

  Future<TwoFactorChallenge?> login(String username, String password) {
    return _authenticate(
      'Auth/Login',
      {'username': username, 'password': password},
      'Invalid username or password.',
    );
  }

  Future<TwoFactorChallenge?> verifyCode(String username, String code) {
    return _authenticate(
      'Auth/Verify',
      {'username': username, 'code': code},
      'That code is not correct.',
    );
  }

  Future<TwoFactorChallenge?> _authenticate(
    String path,
    Map<String, String> body,
    String rejectionMessage,
  ) async {
    final response = await http.post(
      Uri.parse('${ApiConfig.baseUrl}$path'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(body),
    );

    if (response.statusCode == 400 || response.statusCode == 401) {
      final parsed = ApiErrorParser.messageFromBody(response.body);
      throw ApiClientException(parsed ?? rejectionMessage);
    }

    if (response.statusCode >= 299) {
      final parsed = ApiErrorParser.messageFromBody(response.body);
      throw ApiClientException(parsed ?? 'Could not sign in. Please try again.');
    }

    final data = jsonDecode(response.body);

    if (data['requiresTwoFactor'] == true) {
      return TwoFactorChallenge(
        maskedEmail: data['maskedEmail'] as String? ?? '',
        expiresInSeconds: data['codeExpiresInSeconds'] as int? ?? 0,
      );
    }

    accessToken = data['accesstoken'];
    refreshToken = data['refreshtoken'];
    _mustChangePassword = data['mustChangePassword'] as bool? ?? false;

    _readClaims();
    notifyListeners();

    return null;
  }

  Future<bool> readTwoFactorSetting() async {
    final response = await http.get(
      Uri.parse('${ApiConfig.baseUrl}Auth/TwoFactor'),
      headers: _authorizedHeaders(),
    );

    if (response.statusCode >= 299) {
      throw ApiClientException(
        ApiErrorParser.messageFromBody(response.body) ??
            'Could not read your security settings.',
      );
    }

    return jsonDecode(response.body)['enabled'] as bool? ?? false;
  }

  Future<bool> setTwoFactorSetting(bool enabled) async {
    final response = await http.put(
      Uri.parse('${ApiConfig.baseUrl}Auth/TwoFactor'),
      headers: _authorizedHeaders(),
      body: jsonEncode({'enabled': enabled}),
    );

    if (response.statusCode >= 299) {
      throw ApiClientException(
        ApiErrorParser.messageFromBody(response.body) ??
            'Could not change your security settings.',
      );
    }

    return jsonDecode(response.body)['enabled'] as bool? ?? false;
  }

  Map<String, String> _authorizedHeaders() => {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ${accessToken ?? ''}',
      };

  Future<void> logout() async {
    final token = accessToken;

    if (token != null) {
      await _revokeServerSessionIgnoringFailure(token);
    }

    clearSession();
  }

  Future<void> _revokeServerSessionIgnoringFailure(String token) async {
    try {
      await http.post(
        Uri.parse('${ApiConfig.baseUrl}Auth/Logout'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );
    } on Exception {
      return;
    }
  }

  void clearSession() {
    accessToken = null;
    refreshToken = null;
    _role = null;
    _displayName = null;
    _userId = null;
    _mustChangePassword = false;
    notifyListeners();
  }

  void _readClaims() {
    final claims = _decodeJwtPayload(accessToken);

    if (claims == null) {
      return;
    }

    _role = claims['Role'] as String?;
    _userId = int.tryParse('${claims['Id']}');

    final first = claims['FirstName'] as String? ?? '';
    final last = claims['LastName'] as String? ?? '';
    final name = [first, last].where((p) => p.isNotEmpty).join(' ');
    _displayName = name.isEmpty ? null : name;
  }

  Map<String, dynamic>? _decodeJwtPayload(String? token) {
    if (token == null) {
      return null;
    }

    final segments = token.split('.');
    if (segments.length != 3) {
      return null;
    }

    try {
      final normalized = base64Url.normalize(segments[1]);
      final decoded = utf8.decode(base64Url.decode(normalized));
      final map = jsonDecode(decoded);
      return map is Map<String, dynamic> ? map : null;
    } catch (_) {
      return null;
    }
  }
}
