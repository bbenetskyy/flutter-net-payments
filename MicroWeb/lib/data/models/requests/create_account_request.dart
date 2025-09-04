//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

import '../currency.dart';

part 'create_account_request.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class CreateAccountRequest {
  /// Returns a new [CreateAccountRequest] instance.
  CreateAccountRequest({this.iban, this.currency});

  @JsonKey(name: r'iban', required: false, includeIfNull: false)
  final String? iban;

  @JsonKey(name: r'currency', required: false, includeIfNull: false)
  final Currency? currency;

  @override
  bool operator ==(Object other) =>
      identical(this, other) || other is CreateAccountRequest && other.iban == iban && other.currency == currency;

  @override
  int get hashCode => (iban == null ? 0 : iban.hashCode) + currency.hashCode;

  factory CreateAccountRequest.fromJson(Map<String, dynamic> json) => _$CreateAccountRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CreateAccountRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
