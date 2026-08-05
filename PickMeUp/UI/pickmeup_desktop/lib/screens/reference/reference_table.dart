import 'package:flutter/material.dart';

class ReferenceTable {
  const ReferenceTable({
    required this.endpoint,
    required this.title,
    required this.itemNoun,
    required this.icon,
    required this.searchHint,
    required this.nameLabel,
    required this.nameMaxLength,
    this.hasPostalCode = false,
    this.hasDescription = false,
    this.hasSortOrder = false,
    required this.createdMessage,
    required this.deleteQuestion,
    required this.deleteWarning,
  });

  final String endpoint;
  final String title;
  final String itemNoun;
  final IconData icon;
  final String searchHint;
  final String nameLabel;
  final int nameMaxLength;
  final bool hasPostalCode;
  final bool hasDescription;
  final bool hasSortOrder;
  final String createdMessage;
  final String deleteQuestion;
  final String deleteWarning;

  static const int descriptionMaxLength = 300;
  static const int postalCodeMaxLength = 20;
}

const List<ReferenceTable> referenceTables = [
  ReferenceTable(
    endpoint: 'Cities',
    title: 'Cities',
    itemNoun: 'city',
    icon: Icons.location_city,
    searchHint: 'Search by city name',
    nameLabel: 'City name',
    nameMaxLength: 100,
    hasPostalCode: true,
    createdMessage: 'was added to the list of cities.',
    deleteQuestion: 'Remove this city?',
    deleteWarning:
        'Companies and passengers will no longer be able to choose it. A city already used by existing records cannot be removed.',
  ),
  ReferenceTable(
    endpoint: 'VehicleCategories',
    title: 'Vehicle categories',
    itemNoun: 'vehicle category',
    icon: Icons.directions_car_filled,
    searchHint: 'Search by category name',
    nameLabel: 'Category name',
    nameMaxLength: 100,
    hasDescription: true,
    createdMessage: 'was added to the vehicle categories.',
    deleteQuestion: 'Remove this vehicle category?',
    deleteWarning:
        'Vehicles can no longer be classified under it. A category already used by a vehicle cannot be removed.',
  ),
  ReferenceTable(
    endpoint: 'OrderStatuses',
    title: 'Order statuses',
    itemNoun: 'order status',
    icon: Icons.local_taxi,
    searchHint: 'Search by status name',
    nameLabel: 'Status name',
    nameMaxLength: 50,
    hasDescription: true,
    hasSortOrder: true,
    createdMessage: 'was added to the order statuses.',
    deleteQuestion: 'Remove this order status?',
    deleteWarning:
        'The ride workflow depends on these statuses. One already used by a ride cannot be removed.',
  ),
  ReferenceTable(
    endpoint: 'SupportTicketStatuses',
    title: 'Ticket statuses',
    itemNoun: 'ticket status',
    icon: Icons.support_agent,
    searchHint: 'Search by status name',
    nameLabel: 'Status name',
    nameMaxLength: 50,
    hasDescription: true,
    hasSortOrder: true,
    createdMessage: 'was added to the ticket statuses.',
    deleteQuestion: 'Remove this ticket status?',
    deleteWarning:
        'Support tickets are filed under these statuses. One already used by a ticket cannot be removed.',
  ),
  ReferenceTable(
    endpoint: 'PaymentStatuses',
    title: 'Payment statuses',
    itemNoun: 'payment status',
    icon: Icons.payments_outlined,
    searchHint: 'Search by status name',
    nameLabel: 'Status name',
    nameMaxLength: 50,
    hasDescription: true,
    hasSortOrder: true,
    createdMessage: 'was added to the payment statuses.',
    deleteQuestion: 'Remove this payment status?',
    deleteWarning:
        'Payments are recorded against these statuses. One already used by a payment cannot be removed.',
  ),
];
