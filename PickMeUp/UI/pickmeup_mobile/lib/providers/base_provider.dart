import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

import '../core/config/api_config.dart';
import '../models/search_result.dart';
import '../utils/api_client_exception.dart';
import 'auth_provider.dart';

abstract class BaseProvider<T> with ChangeNotifier {
  BaseProvider(this.endpoint);

  final String endpoint;

  static String get baseUrl => ApiConfig.baseUrl;

  Future<SearchResult<T>> get({Map<String, dynamic>? filter}) async {
    var url = '$baseUrl$endpoint';
    if (filter != null && filter.isNotEmpty) {
      url = '$url?${getQueryString(filter)}';
    }

    final response = await http.get(Uri.parse(url), headers: createHeaders());
    validateResponse(response);

    final data = jsonDecode(response.body);
    final result = SearchResult<T>();
    result.totalCount = data['totalCount'];
    result.items = List<T>.from(data['items'].map((e) => fromJson(e)));

    return result;
  }

  Future<T> getById(int id) async {
    final response = await http.get(
      Uri.parse('$baseUrl$endpoint/$id'),
      headers: createHeaders(),
    );
    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }

  Future<T> insert(dynamic request) async {
    final response = await http.post(
      Uri.parse('$baseUrl$endpoint'),
      headers: createHeaders(),
      body: jsonEncode(request),
    );
    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }

  Future<T> update(int id, [dynamic request]) async {
    final response = await http.put(
      Uri.parse('$baseUrl$endpoint/$id'),
      headers: createHeaders(),
      body: jsonEncode(request),
    );
    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }

  Future<void> remove(int id) async {
    final response = await http.delete(
      Uri.parse('$baseUrl$endpoint/$id'),
      headers: createHeaders(),
    );
    validateResponse(response);
  }

  T fromJson(dynamic data) {
    throw UnimplementedError('fromJson must be overridden by $runtimeType.');
  }

  void validateResponse(http.Response response) {
    if (response.statusCode < 299) {
      return;
    }

    if (response.statusCode == 401) {
      throw ApiClientException(
        'Your session has expired. Please sign in again.',
        isUnauthorized: true,
      );
    }

    if (response.statusCode == 403) {
      throw ApiClientException('You do not have access to this section.');
    }

    final parsed = ApiErrorParser.messageFromBody(response.body);

    if (response.statusCode >= 500) {
      throw ApiClientException(
        parsed ?? 'Server error. Please try again later.',
      );
    }

    throw ApiClientException(
      parsed ?? 'Request could not be completed. Please try again.',
    );
  }

  Map<String, String> createHeaders() {
    return {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ${AuthProvider.accessToken ?? ''}',
    };
  }

  String getQueryString(Map<String, dynamic> params) {
    final parts = <String>[];

    params.forEach((key, value) {
      if (value == null) {
        return;
      }
      if (value is DateTime) {
        parts.add('$key=${Uri.encodeComponent(value.toIso8601String())}');
      } else {
        parts.add('$key=${Uri.encodeComponent(value.toString())}');
      }
    });

    return parts.join('&');
  }
}
