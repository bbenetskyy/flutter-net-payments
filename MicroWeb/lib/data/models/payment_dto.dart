// Minimal response DTO for Payments without code generation
class PaymentDto {
  PaymentDto({
    required this.id,
    this.beneficiaryName,
    this.beneficiaryAccount,
    this.fromAccount,
    this.amount,
    this.currency,
    this.status,
    this.details,
    this.createdAt,
  });

  final String id;
  final String? beneficiaryName;
  final String? beneficiaryAccount;
  final String? fromAccount;
  final double? amount;
  final String? currency; // keep as string; map in repository
  final String? status;
  final String? details;
  final String? createdAt; // ISO-8601 string if present

  factory PaymentDto.fromJson(Map<String, dynamic> json) {
    double? _toDouble(dynamic v) {
      if (v is num) return v.toDouble();
      if (v is String) return double.tryParse(v);
      return null;
    }

    return PaymentDto(
      id: (json['id'] ?? '').toString(),
      beneficiaryName: (json['beneficiaryName'] ?? json['counterpartyName'])?.toString(),
      beneficiaryAccount: (json['beneficiaryAccount'] ?? json['toAccount'] ?? json['iban'])?.toString(),
      fromAccount: (json['fromAccount'] ?? json['sourceAccount'])?.toString(),
      amount: _toDouble(json['amount'] ?? json['amountMajor'] ?? json['value']),
      currency: (json['currency'] ?? json['ccy'])?.toString(),
      status: (json['status'] ?? json['state'])?.toString(),
      details: (json['details'] ?? json['description'])?.toString(),
      createdAt: (json['createdAt'] ?? json['created'] ?? json['date'])?.toString(),
    );
  }

  Map<String, dynamic> toJson() => {
    'id': id,
    if (beneficiaryName != null) 'beneficiaryName': beneficiaryName,
    if (beneficiaryAccount != null) 'beneficiaryAccount': beneficiaryAccount,
    if (fromAccount != null) 'fromAccount': fromAccount,
    if (amount != null) 'amount': amount,
    if (currency != null) 'currency': currency,
    if (status != null) 'status': status,
    if (details != null) 'details': details,
    if (createdAt != null) 'createdAt': createdAt,
  };
}
