import '../models/support_ticket.dart';
import 'base_provider.dart';

class SupportTicketProvider extends BaseProvider<SupportTicket> {
  SupportTicketProvider() : super('SupportTickets');

  @override
  SupportTicket fromJson(dynamic data) =>
      SupportTicket.fromJson(data as Map<String, dynamic>);
}
