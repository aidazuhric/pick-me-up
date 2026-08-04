import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

import '../core/config/api_config.dart';
import '../utils/api_client_exception.dart';

class AuthProvider with ChangeNotifier {
  static String? accessToken;
  static String? refreshToken;

  String? _role;
  String? _displayName;
  int? _userId;

  String? get role => _role;
  String? get displayName => _displayName;
  int? get userId => _userId;
  bool get isAuthenticated => accessToken != null;

  Future<void> login(String username, String password) async {
    final response = await http.post(
      Uri.parse('${ApiConfig.baseUrl}Auth/Login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'username': username, 'password': password}),
    );

    if (response.statusCode == 400 || response.statusCode == 401) {
      final parsed = ApiErrorParser.messageFromBody(response.body);
      throw ApiClientException(parsed ?? 'Invalid username or password.');
    }

    if (response.statusCode >= 299) {
      final parsed = ApiErrorParser.messageFromBody(response.body);
      throw ApiClientException(parsed ?? 'Could not sign in. Please try again.');
    }

    final data = jsonDecode(response.body);
    accessToken = data['accesstoken'];
    refreshToken = data['refreshtoken'];

    _readClaims();
    notifyListeners();
  }

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
