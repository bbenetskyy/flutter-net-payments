//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

import '../currency.dart';

part 'create_payment_request.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class CreatePaymentRequest {
  /// Returns a new [CreatePaymentRequest] instance.
  CreatePaymentRequest({
    this.beneficiaryName,

    this.beneficiaryAccount,

    this.fromAccount,

    this.amount,

    this.currency,

    this.details,
  });

  @JsonKey(name: r'beneficiaryName', required: false, includeIfNull: false)
  final String? beneficiaryName;

  @JsonKey(name: r'beneficiaryAccount', required: false, includeIfNull: false)
  final String? beneficiaryAccount;

  @JsonKey(name: r'fromAccount', required: false, includeIfNull: false)
  final String? fromAccount;

  @JsonKey(name: r'amount', required: false, includeIfNull: false)
  final double? amount;

  @JsonKey(name: r'currency', required: false, includeIfNull: false)
  final Currency? currency;

  @JsonKey(name: r'details', required: false, includeIfNull: false)
  final String? details;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CreatePaymentRequest &&
          other.beneficiaryName == beneficiaryName &&
          other.beneficiaryAccount == beneficiaryAccount &&
          other.fromAccount == fromAccount &&
          other.amount == amount &&
          other.currency == currency &&
          other.details == details;

  @override
  int get hashCode =>
      (beneficiaryName == null ? 0 : beneficiaryName.hashCode) +
      (beneficiaryAccount == null ? 0 : beneficiaryAccount.hashCode) +
      (fromAccount == null ? 0 : fromAccount.hashCode) +
      amount.hashCode +
      currency.hashCode +
      (details == null ? 0 : details.hashCode);

  factory CreatePaymentRequest.fromJson(Map<String, dynamic> json) => _$CreatePaymentRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CreatePaymentRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
