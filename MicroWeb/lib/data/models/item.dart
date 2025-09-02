import 'package:equatable/equatable.dart';

class Item extends Equatable {
  const Item({
    required this.id,
    required this.title,
    this.subtitle = '',
    this.description = '',
    this.amount,
    this.currency,
    this.credit,
    this.status,
    this.account,
    this.date,
  });

  final String id;
  final String title;
  final String subtitle;
  final String description;

  // Optional payment-specific fields used by the payments repository mapping
  final double? amount;
  final String? currency;
  final bool? credit;
  final String? status;
  final String? account;
  final DateTime? date;

  Item copyWith({
    String? id,
    String? title,
    String? subtitle,
    String? description,
    double? amount,
    String? currency,
    bool? credit,
    String? status,
    String? account,
    DateTime? date,
  }) => Item(
    id: id ?? this.id,
    title: title ?? this.title,
    subtitle: subtitle ?? this.subtitle,
    description: description ?? this.description,
    amount: amount ?? this.amount,
    currency: currency ?? this.currency,
    credit: credit ?? this.credit,
    status: status ?? this.status,
    account: account ?? this.account,
    date: date ?? this.date,
  );

  @override
  List<Object?> get props => [id, title, subtitle, description, amount, currency, credit, status, account, date];
}
