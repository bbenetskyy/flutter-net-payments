//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

import 'currency.dart';

part 'top_up_request.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class TopUpRequest {
  /// Returns a new [TopUpRequest] instance.
  TopUpRequest({this.amountMinor, this.currency, this.correlationId, this.description});

  @JsonKey(name: r'amountMinor', required: false, includeIfNull: false)
  final int? amountMinor;

  @JsonKey(name: r'currency', required: false, includeIfNull: false)
  final Currency? currency;

  @JsonKey(name: r'correlationId', required: false, includeIfNull: false)
  final String? correlationId;

  @JsonKey(name: r'description', required: false, includeIfNull: false)
  final String? description;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is TopUpRequest &&
          other.amountMinor == amountMinor &&
          other.currency == currency &&
          other.correlationId == correlationId &&
          other.description == description;

  @override
  int get hashCode =>
      amountMinor.hashCode +
      currency.hashCode +
      (correlationId == null ? 0 : correlationId.hashCode) +
      (description == null ? 0 : description.hashCode);

  factory TopUpRequest.fromJson(Map<String, dynamic> json) => _$TopUpRequestFromJson(json);

  Map<String, dynamic> toJson() => _$TopUpRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
