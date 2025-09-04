//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';
import 'package:micro_web/data/models/verification_action.dart';

part 'create_verification_request.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class CreateVerificationRequest {
  /// Returns a new [CreateVerificationRequest] instance.
  CreateVerificationRequest({this.action, this.targetId});

  @JsonKey(name: r'action', required: false, includeIfNull: false)
  final VerificationAction? action;

  @JsonKey(name: r'targetId', required: false, includeIfNull: false)
  final String? targetId;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CreateVerificationRequest && other.action == action && other.targetId == targetId;

  @override
  int get hashCode => action.hashCode + targetId.hashCode;

  factory CreateVerificationRequest.fromJson(Map<String, dynamic> json) => _$CreateVerificationRequestFromJson(json);

  Map<String, dynamic> toJson() => _$CreateVerificationRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
