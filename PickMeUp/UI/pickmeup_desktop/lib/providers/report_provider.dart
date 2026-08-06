import 'dart:typed_data';

import 'package:http/http.dart' as http;

import '../core/config/api_config.dart';
import '../providers/auth_provider.dart';
import '../utils/api_client_exception.dart';

class ReportProvider {
  static const String _ordersByPeriod = 'Reports/OrdersByPeriod';
  static const String _topCompanies = 'Reports/TopCompanies';
  static const String _companyBusiness = 'Reports/CompanyBusiness';

  Future<Uint8List> ordersByPeriod({
    required DateTime from,
    required DateTime to,
    required String granularity,
  }) {
    return _download(_ordersByPeriod, {
      'from': _date(from),
      'to': _date(to),
      'granularity': granularity,
    });
  }

  Future<Uint8List> topCompanies({
    required DateTime from,
    required DateTime to,
  }) {
    return _download(_topCompanies, {'from': _date(from), 'to': _date(to)});
  }

  Future<Uint8List> companyBusiness({
    required DateTime from,
    required DateTime to,
  }) {
    return _download(_companyBusiness, {'from': _date(from), 'to': _date(to)});
  }

  Future<Uint8List> _download(String path, Map<String, String> query) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}$path')
        .replace(queryParameters: query);

    final response = await http.get(uri, headers: {
      'Authorization': 'Bearer ${AuthProvider.accessToken ?? ''}',
    });

    if (response.statusCode == 401) {
      throw ApiClientException(
        'Your session has expired. Please sign in again.',
        isUnauthorized: true,
      );
    }

    if (response.statusCode == 403) {
      throw ApiClientException('Only an administrator can open this report.');
    }

    if (response.statusCode >= 299) {
      final parsed = ApiErrorParser.messageFromBody(response.body);
      throw ApiClientException(
        parsed ?? 'The report could not be generated. Please try again.',
      );
    }

    return response.bodyBytes;
  }

  static String _date(DateTime value) {
    String two(int v) => v.toString().padLeft(2, '0');
    return '${value.year}-${two(value.month)}-${two(value.day)}';
  }
}
