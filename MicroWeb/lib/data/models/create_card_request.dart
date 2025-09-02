//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

import 'card_type.dart';

part 'create_card_request.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class CreateCardRequest {
  /// Returns a new [CreateCardRequest] instance.
  CreateCardRequest({this.type, this.name, this.singleTransactionLimit, this.monthlyLimit});

  @JsonKey(name: r'type', required: false, includeIfNull: false)
  final CardType? type;

  @JsonKey(name: r'name', required: false, includeIfNull: false)
  final String? name;

  @JsonKey(name: r'singleTransactionLimit', required: false, includeIfNull: false)
  final double? singleTransactionLimit;

  @JsonKey(name: r'monthlyLimit', required: false, includeIfNull: false)
  final double? monthlyLimit;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CreateCardRequest &&
          other.type == type &&
          other.name == name &&
          other.singleTransactionLimit == singleTransactionLimit &&
          other.monthlyLimit == monthlyLimit;

  @override
  int get hashCode =>
      type.hashCode + (name == null ? 0 : name.hashCode) + singleTransactionLimit.hashCode + monthlyLimit.hashCode;

  factory CreateCardRequest.fromJson(Map<String, dynamic> json) => _$CreateCardRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CreateCardRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
