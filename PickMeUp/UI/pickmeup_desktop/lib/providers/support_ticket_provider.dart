import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/support_ticket.dart';
import 'base_provider.dart';

class SupportTicketProvider extends BaseProvider<SupportTicket> {
  SupportTicketProvider() : super('SupportTickets');

  @override
  SupportTicket fromJson(dynamic data) =>
      SupportTicket.fromJson(data as Map<String, dynamic>);

  Future<SupportTicket> reply(int id, String body) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$id/Replies'),
      headers: createHeaders(),
      body: jsonEncode({'body': body}),
    );

    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }

  Future<SupportTicket> changeStatus(int id, String status) async {
    final response = await http.post(
      Uri.parse('${BaseProvider.baseUrl}$endpoint/$id/Status'),
      headers: createHeaders(),
      body: jsonEncode({'status': status}),
    );

    validateResponse(response);

    return fromJson(jsonDecode(response.body));
  }
}
