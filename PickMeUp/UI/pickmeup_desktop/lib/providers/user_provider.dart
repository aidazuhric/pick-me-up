import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/user.dart';
import 'base_provider.dart';

class UserProvider extends BaseProvider<User> {
  UserProvider() : super('Users');

  @override
  User fromJson(dynamic data) => User.fromJson(data as Map<String, dynamic>);

  Future<void> changePassword({
    required int id,
    required String currentPassword,
    required String newPassword,
    required String confirmPassword,
  }) async {
    final response = await http.put(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/ChangePassword'),
      headers: createHeaders(),
      body: jsonEncode({
        'id': id,
        'password': currentPassword,
        'newPassword': newPassword,
        'confirmNewPassword': confirmPassword,
      }),
    );

    validateResponse(response);
  }
}
