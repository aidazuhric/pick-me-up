import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:http_parser/http_parser.dart';

import '../models/user.dart';
import 'auth_provider.dart';
import 'base_provider.dart';

class UserProvider extends BaseProvider<User> {
  UserProvider() : super('Users');

  @override
  User fromJson(dynamic data) => User.fromJson(data as Map<String, dynamic>);

  Future<User> readMe() async {
    final response = await http.get(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Me'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return User.fromJson(jsonDecode(response.body));
  }

  Future<User> updateMe({
    required String firstName,
    required String lastName,
    required String email,
    String? phoneNumber,
  }) async {
    final response = await http.put(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Me'),
      headers: createHeaders(),
      body: jsonEncode({
        'firstName': firstName,
        'lastName': lastName,
        'email': email,
        'phoneNumber': phoneNumber,
      }),
    );
    validateResponse(response);

    return User.fromJson(jsonDecode(response.body));
  }

  Future<void> deleteMe() async {
    final response = await http.delete(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Me'),
      headers: createHeaders(),
    );
    validateResponse(response);
  }

  Future<void> uploadOwnPhoto(String filePath, String mimeType) async {
    final request = http.MultipartRequest(
      'POST',
      Uri.parse('${BaseProvider.baseUrl}$endpoint/Me/Photo'),
    );

    request.headers.addAll({
      'Authorization': 'Bearer ${AuthProvider.accessToken ?? ''}',
    });

    request.files.add(
      await http.MultipartFile.fromPath(
        'file',
        filePath,
        contentType: MediaType.parse(mimeType),
      ),
    );

    final streamed = await request.send();
    final response = await http.Response.fromStream(streamed);

    validateResponse(response);
  }

  Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
    required String confirmPassword,
  }) async {
    final response = await http.put(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/ChangePassword'),
      headers: createHeaders(),
      body: jsonEncode({
        'password': currentPassword,
        'newPassword': newPassword,
        'confirmNewPassword': confirmPassword,
      }),
    );
    validateResponse(response);
  }
}
